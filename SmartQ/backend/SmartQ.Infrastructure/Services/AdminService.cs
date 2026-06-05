using Microsoft.EntityFrameworkCore;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;
using SmartQ.Domain.Entities;
using SmartQ.Domain.Enums;
using SmartQ.Infrastructure.Persistence;

namespace SmartQ.Infrastructure.Services;

public class AdminService : IAdminService
{
    private static readonly string[] DistributionColors = ["#006a66", "#07006c", "#d3e4fe", "#81f2eb", "#45464d", "#0b1c30"];

    private readonly SmartQDbContext _db;
    private readonly IPasswordHashService _passwords;
    private readonly IStaffSessionService _sessions;

    public AdminService(SmartQDbContext db, IPasswordHashService passwords, IStaffSessionService sessions)
    {
        _db = db;
        _passwords = passwords;
        _sessions = sessions;
    }

    // ─── Profile ─────────────────────────────────────────────────────────────

    public async Task<AdminProfileDto> GetProfileAsync(CancellationToken ct = default)
    {
        var admin = await _db.StaffUsers.AsNoTracking()
            .Where(s => s.IsActive && s.Role == StaffRole.ADMIN)
            .OrderBy(s => s.Id)
            .Select(s => new AdminProfileDto(s.FullName, s.Role.ToString()))
            .FirstOrDefaultAsync(ct);

        return admin ?? new AdminProfileDto("Admin User", "ADMIN");
    }

    // ─── Dashboard ─────────────────────────────────────────────────────────────

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var yesterday = today.AddDays(-1);

        var activeTokens = await _db.Tokens.CountAsync(
            t => t.Status == TokenStatus.WAITING || t.Status == TokenStatus.CALLED || t.Status == TokenStatus.SERVING, ct);

        var tokensToday = await _db.Tokens.CountAsync(t => t.CreatedAt >= today && t.CreatedAt < tomorrow, ct);
        var tokensYesterday = await _db.Tokens.CountAsync(
            t => t.CreatedAt >= yesterday && t.CreatedAt < today, ct);

        var trendPercent = tokensYesterday == 0
            ? 0
            : Math.Round((double)(tokensToday - tokensYesterday) / tokensYesterday * 100, 1);

        var avgWait = await _db.Tokens.AsNoTracking()
            .Where(t => t.Status == TokenStatus.WAITING)
            .Select(t => (double?)t.EstimatedWaitMinutes)
            .AverageAsync(ct) ?? 0;

        var prevWait = await _db.Tokens.AsNoTracking()
            .Where(t => t.Status == TokenStatus.WAITING && t.CreatedAt < today)
            .Select(t => (double?)t.EstimatedWaitMinutes)
            .AverageAsync(ct) ?? 0;

        var staffOnline = await _db.StaffUsers.CountAsync(s => s.IsActive && s.CounterId != null, ct);
        var staffTotal = await _db.StaffUsers.CountAsync(ct);

        var satisfactionRate = await GetSettingDoubleAsync("SATISFACTION_RATE", ct);

        var from24h = DateTime.Now.AddHours(-24);
        var hourlyTokens = await _db.Tokens.AsNoTracking()
            .Where(t => t.CreatedAt >= from24h)
            .Select(t => new { Hour = t.CreatedAt.Hour, IsPriority = t.Priority != TokenPriority.STANDARD })
            .ToListAsync(ct);

        var hourlyFlow = Enumerable.Range(0, 24).Select(h =>
        {
            var bucket = hourlyTokens.Where(t => t.Hour == h).ToList();
            return new HourlyFlowDto(
                $"{h:D2}:00",
                bucket.Count(t => !t.IsPriority),
                bucket.Count(t => t.IsPriority));
        }).ToList();

        // Service-wise token distribution from DB
        var serviceDist = await (
            from t in _db.Tokens.AsNoTracking()
            where t.CreatedAt >= today && t.CreatedAt < tomorrow
            join s in _db.Services.AsNoTracking() on t.ServiceId equals s.Id
            group t by new { s.Name } into g
            select new { g.Key.Name, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(ct);

        var distribution = serviceDist.Select((s, i) =>
            new TokenDistributionDto(s.Name, s.Count, DistributionColors[i % DistributionColors.Length])).ToList();

        var activities = new List<ActivityItemDto>
        {
            new("System Status", "Queue system is operational.", "success", "Just now")
        };

        var pendingCount = await _db.Tokens.CountAsync(t => t.Status == TokenStatus.WAITING, ct);
        if (pendingCount > 10)
            activities.Add(new ActivityItemDto(
                "High Queue Volume",
                $"{pendingCount} customers waiting across all counters.",
                "error", "Live"));

        var recentCompleted = await _db.Tokens.AsNoTracking()
            .Where(t => t.CompletedAt >= DateTime.Now.AddHours(-8))
            .OrderByDescending(t => t.CompletedAt)
            .Take(2)
            .Select(t => new { t.TokenNo, ServiceName = t.Service.Name, t.CompletedAt })
            .ToListAsync(ct);

        foreach (var tok in recentCompleted)
            activities.Add(new ActivityItemDto(
                $"Token {tok.TokenNo} Completed",
                $"{tok.ServiceName} service completed.",
                "success",
                FormatTimeAgo(tok.CompletedAt!.Value)));

        var counterStatuses = await BuildCounterStatusesAsync(ct);

        return new DashboardSummaryDto(
            activeTokens, avgWait, staffOnline, staffTotal, satisfactionRate,
            tokensToday, Math.Round(avgWait - prevWait, 1), trendPercent,
            hourlyFlow, distribution, activities.Take(4).ToList(), counterStatuses);
    }

    // ─── Service Master ──────────────────────────────────────────────────────

    public async Task<ServiceManagementSummaryDto> GetServiceManagementSummaryAsync(CancellationToken ct = default)
    {
        var (today, tomorrow) = TodayRange();
        var total = await _db.Services.CountAsync(ct);
        var active = await _db.Services.CountAsync(s => s.IsActive, ct);
        var tokensToday = await _db.Tokens.CountAsync(t => t.CreatedAt >= today && t.CreatedAt < tomorrow, ct);
        var avgWait = await _db.Tokens.AsNoTracking()
            .Where(t => t.Status == TokenStatus.WAITING)
            .Select(t => (double?)t.EstimatedWaitMinutes)
            .AverageAsync(ct) ?? 0;
        return new ServiceManagementSummaryDto(total, active, tokensToday, avgWait);
    }

    public async Task<PagedResult<AdminServiceListItemDto>> GetServicesAsync(ServiceListQuery query, CancellationToken ct = default)
    {
        var (today, tomorrow) = TodayRange();
        var q = _db.Services.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(s => s.Name.Contains(term) || s.Code.Contains(term));
        }
        if (query.IsActive.HasValue)
            q = q.Where(s => s.IsActive == query.IsActive.Value);

        var totalCount = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var items = await q
            .OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new AdminServiceListItemDto(
                s.Id,
                s.Code,
                s.Name,
                s.Description,
                s.Icon,
                s.DisplayOrder,
                s.IsActive,
                _db.SubServices.Count(ss => ss.ServiceId == s.Id),
                _db.CounterServiceAssignments.Count(a => a.ServiceId == s.Id && a.IsActive),
                _db.Tokens.Count(t => t.ServiceId == s.Id && t.CreatedAt >= today && t.CreatedAt < tomorrow),
                _db.Tokens
                    .Where(t => t.ServiceId == s.Id && t.CalledAt != null)
                    .Select(t => (double?)EF.Functions.DateDiffMinute(t.CreatedAt, t.CalledAt!.Value))
                    .Average() ?? 0))
            .ToListAsync(ct);

        return new PagedResult<AdminServiceListItemDto>(items, totalCount);
    }

    public async Task<AdminServiceListItemDto> CreateServiceAsync(UpsertServiceRequest request, CancellationToken ct = default)
    {
        ValidateServiceRequest(request);
        if (await _db.Services.AnyAsync(s => s.Code == request.Code.Trim(), ct))
            throw new InvalidOperationException($"Service code '{request.Code}' already exists.");

        var now = DateTime.UtcNow;
        var entity = new Service
        {
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Icon = request.Icon?.Trim() ?? "miscellaneous_services",
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.Services.Add(entity);
        await _db.SaveChangesAsync(ct);
        return (await GetServiceByIdAsync(entity.Id, ct))!;
    }

    public async Task<AdminServiceListItemDto?> UpdateServiceAsync(int id, UpsertServiceRequest request, CancellationToken ct = default)
    {
        ValidateServiceRequest(request);
        var entity = await _db.Services.FindAsync([id], ct);
        if (entity == null) return null;

        if (await _db.Services.AnyAsync(s => s.Code == request.Code.Trim() && s.Id != id, ct))
            throw new InvalidOperationException($"Service code '{request.Code}' already exists.");

        entity.Code = request.Code.Trim().ToUpperInvariant();
        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim() ?? string.Empty;
        entity.Icon = request.Icon?.Trim() ?? entity.Icon;
        entity.DisplayOrder = request.DisplayOrder;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return await GetServiceByIdAsync(id, ct);
    }

    public async Task<AdminServiceListItemDto?> PatchServiceStatusAsync(int id, PatchStatusRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Services.FindAsync([id], ct);
        if (entity == null) return null;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return await GetServiceByIdAsync(id, ct);
    }

    public async Task<bool> DeleteServiceAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Services.FindAsync([id], ct);
        if (entity == null) return false;

        var hasTokens = await _db.Tokens.AnyAsync(t => t.ServiceId == id, ct);
        var hasSubServices = await _db.SubServices.AnyAsync(s => s.ServiceId == id, ct);

        if (hasTokens || hasSubServices)
        {
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        _db.Services.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ─── Sub-Service Master ────────────────────────────────────────────────────

    public async Task<PagedResult<AdminSubServiceListItemDto>> GetSubServicesAsync(SubServiceListQuery query, CancellationToken ct = default)
    {
        var (today, tomorrow) = TodayRange();
        var q = _db.SubServices.AsNoTracking().AsQueryable();

        if (query.ServiceId.HasValue)
            q = q.Where(s => s.ServiceId == query.ServiceId.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(s => s.Name.Contains(term) || s.Code.Contains(term) || s.TokenPrefix.Contains(term));
        }
        if (query.IsActive.HasValue)
            q = q.Where(s => s.IsActive == query.IsActive.Value);

        var totalCount = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var items = await q
            .OrderBy(s => s.ServiceId).ThenBy(s => s.DisplayOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new AdminSubServiceListItemDto(
                s.Id, s.ServiceId, s.Service.Name, s.Code, s.Name, s.Description,
                s.TokenPrefix, s.Icon, s.EstimatedServiceMinutes, s.DisplayOrder, s.IsActive,
                _db.Tokens.Count(t => t.SubServiceId == s.Id && t.CreatedAt >= today && t.CreatedAt < tomorrow)))
            .ToListAsync(ct);

        return new PagedResult<AdminSubServiceListItemDto>(items, totalCount);
    }

    public async Task<AdminSubServiceListItemDto> CreateSubServiceAsync(UpsertSubServiceRequest request, CancellationToken ct = default)
    {
        await ValidateSubServiceRequestAsync(request, null, ct);
        var now = DateTime.UtcNow;
        var entity = new SubService
        {
            ServiceId = request.ServiceId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            TokenPrefix = request.TokenPrefix.Trim().ToUpperInvariant(),
            Icon = request.Icon?.Trim() ?? "label",
            EstimatedServiceMinutes = request.EstimatedServiceMinutes,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.SubServices.Add(entity);
        await _db.SaveChangesAsync(ct);
        return (await GetSubServiceByIdAsync(entity.Id, ct))!;
    }

    public async Task<AdminSubServiceListItemDto?> UpdateSubServiceAsync(int id, UpsertSubServiceRequest request, CancellationToken ct = default)
    {
        await ValidateSubServiceRequestAsync(request, id, ct);
        var entity = await _db.SubServices.FindAsync([id], ct);
        if (entity == null) return null;

        entity.ServiceId = request.ServiceId;
        entity.Code = request.Code.Trim().ToUpperInvariant();
        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim() ?? string.Empty;
        entity.TokenPrefix = request.TokenPrefix.Trim().ToUpperInvariant();
        entity.Icon = request.Icon?.Trim() ?? entity.Icon;
        entity.EstimatedServiceMinutes = request.EstimatedServiceMinutes;
        entity.DisplayOrder = request.DisplayOrder;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return await GetSubServiceByIdAsync(id, ct);
    }

    public async Task<AdminSubServiceListItemDto?> PatchSubServiceStatusAsync(int id, PatchStatusRequest request, CancellationToken ct = default)
    {
        var entity = await _db.SubServices.FindAsync([id], ct);
        if (entity == null) return null;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return await GetSubServiceByIdAsync(id, ct);
    }

    public async Task<bool> DeleteSubServiceAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.SubServices.FindAsync([id], ct);
        if (entity == null) return false;

        if (await _db.Tokens.AnyAsync(t => t.SubServiceId == id, ct))
        {
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        _db.SubServices.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ─── Counter Master ──────────────────────────────────────────────────────

    public async Task<PagedResult<AdminCounterListItemDto>> GetCountersAsync(CancellationToken ct = default)
    {
        var (today, tomorrow) = TodayRange();
        var counters = await _db.Counters.AsNoTracking()
            .OrderBy(c => c.CounterNo)
            .Select(c => new AdminCounterListItemDto(
                c.Id,
                c.CounterNo,
                c.CounterName,
                c.Status.ToString(),
                c.IsActive,
                c.ServiceAssignments.Where(a => a.IsActive).Select(a => a.Service.Name).ToList(),
                c.StaffUsers.Where(s => s.IsActive).OrderBy(s => s.Id).Select(s => s.FullName).FirstOrDefault(),
                c.Tokens
                    .Where(t => t.Status == TokenStatus.CALLED || t.Status == TokenStatus.SERVING)
                    .OrderByDescending(t => t.CalledAt)
                    .Select(t => t.TokenNo)
                    .FirstOrDefault(),
                c.Tokens.Count(t => t.CreatedAt >= today && t.CreatedAt < tomorrow)))
            .ToListAsync(ct);

        return new PagedResult<AdminCounterListItemDto>(counters, counters.Count);
    }

    public async Task<CounterManagementDto> GetCounterManagementAsync(CancellationToken ct = default)
    {
        var (today, tomorrow) = TodayRange();
        var feedbackScore = await GetSettingDoubleAsync("COUNTER_FEEDBACK_SCORE", ct);

        var counters = await _db.Counters.AsNoTracking()
            .Include(c => c.ServiceAssignments).ThenInclude(a => a.Service)
            .Include(c => c.StaffUsers)
            .Include(c => c.Tokens)
            .OrderBy(c => c.CounterNo)
            .ToListAsync(ct);

        var totalCounters = counters.Count;
        var activeCounters = counters.Count(c =>
            c.IsActive && c.Status is not CounterStatus.OFFLINE and not CounterStatus.MAINTENANCE);
        var staffLive = counters.SelectMany(c => c.StaffUsers).Count(s => s.IsActive);

        var completedToday = await _db.Tokens.AsNoTracking()
            .Where(t => t.CompletedAt >= today && t.CompletedAt < tomorrow && t.StartedAt != null)
            .Select(t => (double?)EF.Functions.DateDiffSecond(t.StartedAt!.Value, t.CompletedAt!.Value))
            .ToListAsync(ct);

        var avgServiceSec = completedToday.Count > 0 ? completedToday.Average() ?? 0 : 0;
        var avgServiceStr = avgServiceSec > 0 ? $"{(int)(avgServiceSec / 60)} min" : "--";

        var pendingTickets = await _db.Tokens.CountAsync(t => t.Status == TokenStatus.WAITING, ct);
        var peakLoad = pendingTickets > 50 ? "High Traffic" : pendingTickets > 20 ? "Moderate" : "Normal";

        var summary = new CounterManagementSummaryDto(
            activeCounters, totalCounters, staffLive,
            totalCounters == 0 ? 0 : (int)Math.Min(100, Math.Round((double)staffLive / totalCounters * 100)),
            avgServiceStr, pendingTickets, peakLoad);

        var cards = new List<CounterManagementCardDto>();
        foreach (var c in counters)
        {
            var serving = c.Tokens.FirstOrDefault(t => t.Status is TokenStatus.CALLED or TokenStatus.SERVING);
            var estMin = serving != null
                ? await _db.SubServices.AsNoTracking()
                    .Where(ss => ss.Id == serving.SubServiceId)
                    .Select(ss => ss.EstimatedServiceMinutes)
                    .FirstOrDefaultAsync(ct)
                : 0;
            cards.Add(BuildCounterCard(c, today, feedbackScore, estMin));
        }
        return new CounterManagementDto(summary, cards);
    }

    public async Task<CounterResourceLoadDto> GetCounterResourceLoadAsync(CancellationToken ct = default)
    {
        var counters = await _db.Counters.AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new
            {
                c.CounterName,
                Waiting = c.Tokens.Count(t => t.Status == TokenStatus.WAITING),
                Staff = c.StaffUsers.Count(s => s.IsActive)
            })
            .ToListAsync(ct);

        var maxWaiting = Math.Max(1, counters.Max(c => c.Waiting));
        var loads = counters.Select(c => new ResourceLoadPointDto(
            c.CounterName,
            (int)Math.Min(100, Math.Round((double)c.Waiting / maxWaiting * 100)),
            100)).ToList();

        var mostLoaded = loads.MaxBy(l => l.AllocatedPercent);
        var leastLoaded = loads.MinBy(l => l.AllocatedPercent);
        var recommendation = mostLoaded != null && leastLoaded != null && mostLoaded.Label != leastLoaded.Label
            ? $"Consider rebalancing load from {leastLoaded.Label} to {mostLoaded.Label}."
            : "Resource allocation looks balanced.";

        return new CounterResourceLoadDto(loads, new CounterOptimizationDto(recommendation, true));
    }

    public async Task<AdminCounterListItemDto> CreateCounterAsync(UpsertCounterRequest request, CancellationToken ct = default)
    {
        ValidateCounterRequest(request);
        if (await _db.Counters.AnyAsync(c => c.CounterNo == request.CounterNo.Trim(), ct))
            throw new InvalidOperationException($"Counter number '{request.CounterNo}' already exists.");

        var now = DateTime.UtcNow;
        var counter = new Counter
        {
            CounterNo = request.CounterNo.Trim(),
            CounterName = request.CounterName.Trim(),
            Status = ParseCounterStatus(request.Status),
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.Counters.Add(counter);
        await _db.SaveChangesAsync(ct);
        return (await GetCounterByIdAsync(counter.Id, ct))!;
    }

    public async Task<AdminCounterListItemDto?> UpdateCounterAsync(int id, UpsertCounterRequest request, CancellationToken ct = default)
    {
        ValidateCounterRequest(request);
        var counter = await _db.Counters.FindAsync([id], ct);
        if (counter == null) return null;

        if (await _db.Counters.AnyAsync(c => c.CounterNo == request.CounterNo.Trim() && c.Id != id, ct))
            throw new InvalidOperationException($"Counter number '{request.CounterNo}' already exists.");

        counter.CounterNo = request.CounterNo.Trim();
        counter.CounterName = request.CounterName.Trim();
        counter.Status = ParseCounterStatus(request.Status);
        counter.IsActive = request.IsActive;
        counter.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return await GetCounterByIdAsync(id, ct);
    }

    public async Task<AdminCounterListItemDto?> PatchCounterStatusAsync(int id, PatchCounterStatusRequest request, CancellationToken ct = default)
    {
        var counter = await _db.Counters.FindAsync([id], ct);
        if (counter == null) return null;
        counter.Status = ParseCounterStatus(request.Status);
        counter.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return await GetCounterByIdAsync(id, ct);
    }

    public async Task<bool> DeleteCounterAsync(int id, CancellationToken ct = default)
    {
        var counter = await _db.Counters.FindAsync([id], ct);
        if (counter == null) return false;

        if (await _db.Tokens.AnyAsync(t => t.CounterId == id, ct))
        {
            counter.IsActive = false;
            counter.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        var assignments = await _db.CounterServiceAssignments.Where(a => a.CounterId == id).ToListAsync(ct);
        _db.CounterServiceAssignments.RemoveRange(assignments);
        _db.Counters.Remove(counter);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ─── Counter-Service Assignment ────────────────────────────────────────────

    public async Task<IReadOnlyList<CounterAssignmentListItemDto>> GetCounterAssignmentsAsync(CancellationToken ct = default)
    {
        return await _db.Counters.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CounterNo)
            .Select(c => new CounterAssignmentListItemDto(
                c.Id,
                c.CounterNo,
                c.CounterName,
                c.ServiceAssignments.Where(a => a.IsActive).Select(a => a.ServiceId).ToList(),
                c.ServiceAssignments.Where(a => a.IsActive).Select(a => a.Service.Name).ToList()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AssignableServiceDto>> GetAssignableServicesAsync(int counterId, CancellationToken ct = default)
    {
        var assignedIds = (await _db.CounterServiceAssignments.AsNoTracking()
            .Where(a => a.CounterId == counterId && a.IsActive)
            .Select(a => a.ServiceId)
            .ToListAsync(ct)).ToHashSet();

        var services = await _db.Services.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new
            {
                s.Id, s.Code, s.Name, s.IsActive,
                Prefixes = s.SubServices.Where(ss => ss.IsActive).Select(ss => ss.TokenPrefix).ToList()
            })
            .ToListAsync(ct);

        return services.Select(s => new AssignableServiceDto(
            s.Id, s.Code, s.Name, s.IsActive, assignedIds.Contains(s.Id), s.Prefixes)).ToList();
    }

    public async Task SaveCounterAssignmentsAsync(SaveCounterAssignmentRequest request, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var counter = await _db.Counters.FindAsync([request.CounterId], ct)
            ?? throw new InvalidOperationException("Counter not found.");

        var existing = await _db.CounterServiceAssignments
            .Where(a => a.CounterId == request.CounterId)
            .ToListAsync(ct);

        var desired = request.ServiceIds.Distinct().ToHashSet();

        foreach (var row in existing)
            row.IsActive = desired.Contains(row.ServiceId);

        foreach (var serviceId in desired)
        {
            if (existing.Any(e => e.ServiceId == serviceId)) continue;
            _db.CounterServiceAssignments.Add(new CounterServiceAssignment
            {
                CounterId = request.CounterId,
                ServiceId = serviceId,
                IsActive = true
            });
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    // ─── Settings ────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SystemSettingDto>> GetSettingsAsync(CancellationToken ct = default)
    {
        return await _db.SystemSettings.AsNoTracking()
            .Where(s => s.IsActive)
            .Select(s => new SystemSettingDto(s.Id, s.SettingKey, s.SettingValue, s.DataType, s.Description))
            .ToListAsync(ct);
    }

    public async Task<SystemSettingDto?> UpdateSettingAsync(int id, UpdateSettingRequest request, CancellationToken ct = default)
    {
        var s = await _db.SystemSettings.FindAsync([id], ct);
        if (s == null) return null;
        s.SettingValue = request.SettingValue;
        await _db.SaveChangesAsync(ct);
        return new SystemSettingDto(s.Id, s.SettingKey, s.SettingValue, s.DataType, s.Description);
    }

    // ─── Reports ─────────────────────────────────────────────────────────────

    public async Task<TokenHistoryReportDto> GetTokenHistoryReportAsync(TokenHistoryFilter filter, CancellationToken ct = default)
    {
        var from = filter.DateFrom ?? DateTime.Today;
        var to = filter.DateTo ?? DateTime.Today.AddDays(1);
        if (to.TimeOfDay == TimeSpan.Zero) to = to.AddDays(1);

        var q = _db.Tokens.AsNoTracking().AsQueryable();
        q = q.Where(t => t.CreatedAt >= from && t.CreatedAt < to);

        if (filter.ServiceId.HasValue) q = q.Where(t => t.ServiceId == filter.ServiceId);
        if (filter.SubServiceId.HasValue) q = q.Where(t => t.SubServiceId == filter.SubServiceId);
        if (filter.CounterId.HasValue) q = q.Where(t => t.CounterId == filter.CounterId);
        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<TokenStatus>(filter.Status, true, out var st))
            q = q.Where(t => t.Status == st);

        var totalCount = await q.CountAsync(ct);
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);

        var completed = await q.CountAsync(t => t.Status == TokenStatus.COMPLETED, ct);
        var skipped = await q.CountAsync(t => t.Status == TokenStatus.SKIPPED || t.Status == TokenStatus.CANCELLED, ct);
        var avgWait = await q.Where(t => t.CalledAt != null)
            .Select(t => (double?)EF.Functions.DateDiffMinute(t.CreatedAt, t.CalledAt!.Value))
            .AverageAsync(ct) ?? 0;

        var items = await q
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TokenHistoryRowDto(
                t.Id, t.TokenNo, t.Service.Name, t.SubService.Name,
                t.Counter != null ? t.Counter.CounterName : null,
                t.CreatedAt, t.CalledAt, t.CompletedAt,
                t.CalledAt != null ? (double?)EF.Functions.DateDiffMinute(t.CreatedAt, t.CalledAt!.Value) : null,
                t.StartedAt != null && t.CompletedAt != null
                    ? (double?)EF.Functions.DateDiffMinute(t.StartedAt!.Value, t.CompletedAt!.Value) : null,
                t.Status.ToString()))
            .ToListAsync(ct);

        // Traffic distribution for chart (all matching tokens, not just page)
        var hourBuckets = await q
            .GroupBy(t => t.CreatedAt.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var timeSlots = new[] { 8, 10, 12, 14, 16 };
        var maxInSlot = Math.Max(1, timeSlots.Max(h =>
            hourBuckets.Where(b => b.Hour >= h && b.Hour < h + 2).Sum(b => b.Count)));

        var traffic = timeSlots.Select(h =>
        {
            var count = hourBuckets.Where(b => b.Hour >= h && b.Hour < h + 2).Sum(b => b.Count);
            return new TrafficDistributionPointDto($"{h:D2}:00", count, (int)Math.Round((double)count / maxInSlot * 100));
        }).ToList();

        var peakSlot = traffic.MaxBy(t => t.Count);
        var peakSummary = peakSlot is { Count: > 0 }
            ? $"Peak activity at {peakSlot.HourLabel} with {peakSlot.Count} tokens."
            : "No significant peak hours in the selected period.";

        var completionRate = totalCount == 0 ? 0 : Math.Round((double)completed / totalCount * 100, 1);
        var insights = new List<OperationalInsightDto>();
        if (peakSlot?.Count > 0)
            insights.Add(new OperationalInsightDto("lightbulb",
                $"Increase staffing during {peakSlot.HourLabel} peak.", "info"));
        insights.Add(new OperationalInsightDto(
            completionRate >= 85 ? "trending_up" : "trending_down",
            $"Completion rate is {completionRate:F1}%.",
            completionRate >= 85 ? "positive" : "warning"));

        return new TokenHistoryReportDto(
            items,
            totalCount,
            new TokenHistorySummaryDto(totalCount, completed, skipped, avgWait),
            traffic,
            peakSummary,
            insights);
    }

    // ─── Staff Management ────────────────────────────────────────────────────

    public async Task<StaffManagementSummaryDto> GetStaffManagementSummaryAsync(CancellationToken ct = default)
    {
        var (today, tomorrow) = TodayRange();
        var total = await _db.StaffUsers.CountAsync(ct);
        var active = await _db.StaffUsers.CountAsync(s => s.IsActive, ct);
        var online = await _db.StaffCounterSessions.CountAsync(
            s => s.Status == StaffCounterSessionStatus.ACTIVE || s.Status == StaffCounterSessionStatus.BREAK, ct);
        var admins = await _db.StaffUsers.CountAsync(s => s.Role == StaffRole.ADMIN, ct);
        return new StaffManagementSummaryDto(total, active, online, admins);
    }

    public async Task<PagedResult<AdminStaffListItemDto>> GetStaffAsync(StaffListQuery query, CancellationToken ct = default)
    {
        var (today, tomorrow) = TodayRange();
        var q = _db.StaffUsers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(s => s.FullName.Contains(term) || s.Username.Contains(term) || s.Email.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(query.Role) && Enum.TryParse<StaffRole>(query.Role, true, out var role))
            q = q.Where(s => s.Role == role);

        if (query.IsActive.HasValue)
            q = q.Where(s => s.IsActive == query.IsActive.Value);

        var totalCount = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var activeStatuses = new[] { StaffCounterSessionStatus.ACTIVE, StaffCounterSessionStatus.BREAK };

        var items = await q
            .OrderBy(s => s.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new AdminStaffListItemDto(
                s.Id,
                s.FullName,
                s.Username,
                s.Email,
                s.Role.ToString(),
                s.IsActive,
                s.CounterSessions
                    .Where(cs => activeStatuses.Contains(cs.Status))
                    .OrderByDescending(cs => cs.StartedAt)
                    .Select(cs => cs.Counter.CounterName)
                    .FirstOrDefault(),
                s.CounterSessions
                    .Where(cs => activeStatuses.Contains(cs.Status))
                    .OrderByDescending(cs => cs.StartedAt)
                    .Select(cs => cs.Status.ToString())
                    .FirstOrDefault(),
                _db.Tokens.Count(t =>
                    t.CompletedAt >= today && t.CompletedAt < tomorrow &&
                    t.CounterId != null &&
                    s.CounterSessions.Any(cs =>
                        cs.CounterId == t.CounterId &&
                        cs.StartedAt <= t.CompletedAt &&
                        (cs.EndedAt == null || cs.EndedAt >= t.CompletedAt)))))
            .ToListAsync(ct);

        return new PagedResult<AdminStaffListItemDto>(items, totalCount);
    }

    public async Task<AdminStaffListItemDto> CreateStaffAsync(CreateStaffRequest request, CancellationToken ct = default)
    {
        ValidateStaffRequest(request.FullName, request.Username, request.Email, request.Role, request.Password);

        if (await _db.StaffUsers.AnyAsync(s => s.Username == request.Username.Trim(), ct))
            throw new InvalidOperationException($"Username '{request.Username}' already exists.");
        if (!string.IsNullOrWhiteSpace(request.Email) &&
            await _db.StaffUsers.AnyAsync(s => s.Email == request.Email.Trim(), ct))
            throw new InvalidOperationException($"Email '{request.Email}' already exists.");

        if (!Enum.TryParse<StaffRole>(request.Role, true, out var role))
            throw new InvalidOperationException("Role must be ADMIN or STAFF.");

        var now = DateTime.UtcNow;
        var user = new StaffUser
        {
            FullName = request.FullName.Trim(),
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = _passwords.HashPassword(request.Password),
            Role = role,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.StaffUsers.Add(user);
        await _db.SaveChangesAsync(ct);
        return (await GetStaffAsync(new StaffListQuery(null, null, null, 1, 1), ct)).Items
            .FirstOrDefault(s => s.Id == user.Id)
            ?? throw new InvalidOperationException("Failed to load created staff user.");
    }

    public async Task<AdminStaffListItemDto?> UpdateStaffAsync(int id, UpdateStaffRequest request, CancellationToken ct = default)
    {
        ValidateStaffRequest(request.FullName, request.Username, request.Email, request.Role, null);

        var user = await _db.StaffUsers.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (user == null) return null;

        if (await _db.StaffUsers.AnyAsync(s => s.Username == request.Username.Trim() && s.Id != id, ct))
            throw new InvalidOperationException($"Username '{request.Username}' already exists.");
        if (!string.IsNullOrWhiteSpace(request.Email) &&
            await _db.StaffUsers.AnyAsync(s => s.Email == request.Email.Trim() && s.Id != id, ct))
            throw new InvalidOperationException($"Email '{request.Email}' already exists.");

        if (!Enum.TryParse<StaffRole>(request.Role, true, out var role))
            throw new InvalidOperationException("Role must be ADMIN or STAFF.");

        user.FullName = request.FullName.Trim();
        user.Username = request.Username.Trim();
        user.Email = request.Email.Trim();
        user.Role = role;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return (await GetStaffAsync(new StaffListQuery(null, null, null, 1, 200), ct)).Items
            .FirstOrDefault(s => s.Id == id);
    }

    public async Task<bool> ResetStaffPasswordAsync(int id, ResetPasswordRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword))
            throw new InvalidOperationException("New password is required.");

        var user = await _db.StaffUsers.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (user == null) return false;

        user.PasswordHash = _passwords.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<AdminStaffListItemDto?> PatchStaffStatusAsync(int id, PatchStaffStatusRequest request, CancellationToken ct = default)
    {
        var user = await _db.StaffUsers.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (user == null) return null;

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return (await GetStaffAsync(new StaffListQuery(null, null, null, 1, 200), ct)).Items
            .FirstOrDefault(s => s.Id == id);
    }

    public async Task<bool> ForceLogoutStaffAsync(int id, CancellationToken ct = default)
    {
        var user = await _db.StaffUsers.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (user == null) return false;

        await _sessions.ForceCloseSessionAsync(id, ct);
        return true;
    }

    private static void ValidateStaffRequest(string fullName, string username, string email, string role, string? password)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new InvalidOperationException("Full name is required.");
        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidOperationException("Username is required.");
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Email is required.");
        if (string.IsNullOrWhiteSpace(role))
            throw new InvalidOperationException("Role is required.");
        if (password != null && string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Password is required.");
    }

    // ─── Private helpers ─────────────────────────────────────────────────────

    private static (DateTime Today, DateTime Tomorrow) TodayRange()
    {
        var today = DateTime.Today;
        return (today, today.AddDays(1));
    }

    private async Task<double?> GetSettingDoubleAsync(string key, CancellationToken ct)
    {
        var value = await _db.SystemSettings.AsNoTracking()
            .Where(s => s.SettingKey == key && s.IsActive)
            .Select(s => s.SettingValue)
            .FirstOrDefaultAsync(ct);
        return double.TryParse(value, out var d) ? d : null;
    }

    private async Task<AdminServiceListItemDto?> GetServiceByIdAsync(int id, CancellationToken ct)
    {
        var result = await GetServicesAsync(new ServiceListQuery(null, null, 1, 1), ct);
        return await _db.Services.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new AdminServiceListItemDto(
                s.Id, s.Code, s.Name, s.Description, s.Icon, s.DisplayOrder, s.IsActive,
                _db.SubServices.Count(ss => ss.ServiceId == s.Id),
                _db.CounterServiceAssignments.Count(a => a.ServiceId == s.Id && a.IsActive),
                _db.Tokens.Count(t => t.ServiceId == s.Id && t.CreatedAt >= DateTime.Today),
                _db.Tokens.Where(t => t.ServiceId == s.Id && t.CalledAt != null)
                    .Select(t => (double?)EF.Functions.DateDiffMinute(t.CreatedAt, t.CalledAt!.Value))
                    .Average() ?? 0))
            .FirstOrDefaultAsync(ct);
    }

    private async Task<AdminSubServiceListItemDto?> GetSubServiceByIdAsync(int id, CancellationToken ct)
    {
        var (today, tomorrow) = TodayRange();
        return await _db.SubServices.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new AdminSubServiceListItemDto(
                s.Id, s.ServiceId, s.Service.Name, s.Code, s.Name, s.Description,
                s.TokenPrefix, s.Icon, s.EstimatedServiceMinutes, s.DisplayOrder, s.IsActive,
                _db.Tokens.Count(t => t.SubServiceId == s.Id && t.CreatedAt >= today && t.CreatedAt < tomorrow)))
            .FirstOrDefaultAsync(ct);
    }

    private async Task<AdminCounterListItemDto?> GetCounterByIdAsync(int id, CancellationToken ct)
    {
        var (today, tomorrow) = TodayRange();
        return await _db.Counters.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new AdminCounterListItemDto(
                c.Id, c.CounterNo, c.CounterName, c.Status.ToString(), c.IsActive,
                c.ServiceAssignments.Where(a => a.IsActive).Select(a => a.Service.Name).ToList(),
                c.StaffUsers.Where(s => s.IsActive).OrderBy(s => s.Id).Select(s => s.FullName).FirstOrDefault(),
                c.Tokens.Where(t => t.Status == TokenStatus.CALLED || t.Status == TokenStatus.SERVING)
                    .OrderByDescending(t => t.CalledAt).Select(t => t.TokenNo).FirstOrDefault(),
                c.Tokens.Count(t => t.CreatedAt >= today && t.CreatedAt < tomorrow)))
            .FirstOrDefaultAsync(ct);
    }

    private async Task<IReadOnlyList<CounterStatusDto>> BuildCounterStatusesAsync(CancellationToken ct)
    {
        var counters = await _db.Counters.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CounterNo)
            .Take(6)
            .Select(c => new
            {
                c.CounterNo,
                StaffName = c.StaffUsers.Where(s => s.IsActive).OrderBy(s => s.Id).Select(s => s.FullName).FirstOrDefault(),
                c.Status,
                Waiting = c.Tokens.Count(t => t.Status == TokenStatus.WAITING),
                HasVipAssignment = c.ServiceAssignments.Any(a => a.IsActive)
            })
            .ToListAsync(ct);

        var maxWaiting = Math.Max(1, counters.Max(c => c.Waiting));
        return counters.Select(c => new CounterStatusDto(
            $"C-{c.CounterNo}",
            c.StaffName ?? "Unassigned",
            c.Status.ToString(),
            (int)Math.Min(100, Math.Round((double)c.Waiting / maxWaiting * 100)),
            c.HasVipAssignment)).ToList();
    }

    private static CounterManagementCardDto BuildCounterCard(Counter c, DateTime today, double? feedbackScore, int estimatedMinutes)
    {
        var serving = c.Tokens.FirstOrDefault(t => t.Status is TokenStatus.CALLED or TokenStatus.SERVING);
        var todayVol = c.Tokens.Count(t => t.CreatedAt >= today && t.CreatedAt < today.AddDays(1));
        var staff = c.StaffUsers.FirstOrDefault(s => s.IsActive);

        int progress = 0;
        string waitLabel = "--";
        string? waitLimit = null;
        var est = estimatedMinutes > 0 ? estimatedMinutes : 1;

        if (serving?.StartedAt != null)
        {
            var elapsed = (DateTime.Now - serving.StartedAt.Value).TotalMinutes;
            progress = (int)Math.Min(100, elapsed / est * 100);
            waitLabel = $"{(int)elapsed:D2}:{(int)((elapsed % 1) * 60):D2}";
            waitLimit = $"{est}:00";
        }
        else if (serving?.CalledAt != null)
        {
            var elapsed = (DateTime.Now - serving.CalledAt.Value).TotalMinutes;
            progress = (int)Math.Min(100, elapsed / est * 100);
            waitLabel = $"{(int)elapsed:D2}:{(int)((elapsed % 1) * 60):D2}";
            waitLimit = $"{est}:00";
        }

        var unitName = c.ServiceAssignments.Where(a => a.IsActive).Select(a => a.Service.Name).FirstOrDefault()
                       ?? c.CounterName;
        var offline = c.Status is CounterStatus.OFFLINE or CounterStatus.MAINTENANCE;
        var offlineMsg = c.Status == CounterStatus.MAINTENANCE ? "Maintenance Required"
            : c.Status == CounterStatus.OFFLINE ? "Counter Offline" : null;

        return new CounterManagementCardDto(
            c.Id, c.CounterNo, c.CounterName, unitName, c.Status.ToString(),
            staff?.FullName, staff?.Role.ToString(), serving?.TokenNo,
            progress, waitLabel, waitLimit, todayVol, feedbackScore, offline, offlineMsg);
    }

    private static void ValidateServiceRequest(UpsertServiceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new InvalidOperationException("Service code is required.");
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException("Service name is required.");
        if (request.DisplayOrder < 0)
            throw new InvalidOperationException("Display order must be >= 0.");
    }

    private async Task ValidateSubServiceRequestAsync(UpsertSubServiceRequest request, int? excludeId, CancellationToken ct)
    {
        if (!await _db.Services.AnyAsync(s => s.Id == request.ServiceId, ct))
            throw new InvalidOperationException("Parent service does not exist.");
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new InvalidOperationException("Sub-service code is required.");
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException("Sub-service name is required.");
        if (string.IsNullOrWhiteSpace(request.TokenPrefix))
            throw new InvalidOperationException("Token prefix is required.");
        if (request.EstimatedServiceMinutes <= 0)
            throw new InvalidOperationException("Estimated service minutes must be > 0.");

        var codeExists = await _db.SubServices.AnyAsync(s =>
            s.ServiceId == request.ServiceId && s.Code == request.Code.Trim() &&
            (!excludeId.HasValue || s.Id != excludeId.Value), ct);
        if (codeExists)
            throw new InvalidOperationException($"Code '{request.Code}' already exists for this service.");
    }

    private static void ValidateCounterRequest(UpsertCounterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CounterNo))
            throw new InvalidOperationException("Counter number is required.");
        if (string.IsNullOrWhiteSpace(request.CounterName))
            throw new InvalidOperationException("Counter name is required.");
    }

    private static CounterStatus ParseCounterStatus(string status) =>
        Enum.TryParse<CounterStatus>(status, true, out var s) ? s : CounterStatus.AVAILABLE;

    private static string FormatTimeAgo(DateTime dt)
    {
        var diff = DateTime.Now - dt;
        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hour(s) ago";
        return $"{(int)diff.TotalDays} day(s) ago";
    }
}
