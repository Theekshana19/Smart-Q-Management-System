using Microsoft.EntityFrameworkCore;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;
using SmartQ.Domain.Entities;
using SmartQ.Domain.Enums;
using SmartQ.Infrastructure.Persistence;

namespace SmartQ.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly SmartQDbContext _db;

    public AdminService(SmartQDbContext db) => _db = db;

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default)
    {
        var today = DateTime.Today;
        var activeTokens = await _db.Tokens.CountAsync(t => t.Status == TokenStatus.WAITING || t.Status == TokenStatus.CALLED, ct);
        var tokensToday = await _db.Tokens.CountAsync(t => t.CreatedAt >= today, ct);
        var avgWait = await _db.Tokens.Where(t => t.Status == TokenStatus.WAITING)
            .Select(t => (double?)t.EstimatedWaitMinutes).AverageAsync(ct) ?? 8;
        var staffOnline = await _db.StaffUsers.CountAsync(s => s.IsActive, ct);
        var staffTotal = staffOnline;

        var hourly = Enumerable.Range(8, 11).Select(h => new HourlyFlowDto(
            $"{h:D2}:00", Random.Shared.Next(20, 80), Random.Shared.Next(5, 20))).ToList();

        var distribution = await _db.Tokens.AsNoTracking()
            .Where(t => t.CreatedAt >= today)
            .GroupBy(t => t.Service.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var distDtos = distribution.Select((d, i) => new TokenDistributionDto(
            d.Name, d.Count, i switch { 0 => "#00695c", 1 => "#00346f", _ => "#abc7ff" })).ToList();

        var counters = await _db.Counters.AsNoTracking()
            .Include(c => c.StaffUsers)
            .Where(c => c.IsActive)
            .Take(5)
            .ToListAsync(ct);

        var counterStatuses = counters.Select(c => new CounterStatusDto(
            $"C-{c.CounterNo}", c.StaffUsers.FirstOrDefault()?.FullName ?? "Unassigned",
            c.Status.ToString(), c.Status == CounterStatus.SERVING ? 85 : 20, false)).ToList();

        return new DashboardSummaryDto(
            activeTokens, avgWait, staffOnline, staffTotal, 4.9,
            tokensToday, -4.2, hourly, distDtos,
            new List<ActivityItemDto>
            {
                new("System Update Completed", "v2.4.1 deployed", "success", "12 minutes ago"),
                new("Capacity Warning", "Zone B at 95%", "error", "1 hour ago")
            },
            counterStatuses);
    }

    public async Task<ServiceManagementSummaryDto> GetServiceManagementSummaryAsync(CancellationToken ct = default)
    {
        var today = DateTime.Today;
        var total = await _db.Services.CountAsync(ct);
        var active = await _db.Services.CountAsync(s => s.IsActive, ct);
        var tokensToday = await _db.Tokens.CountAsync(t => t.CreatedAt >= today, ct);
        var avgWait = await _db.Tokens.Where(t => t.Status == TokenStatus.WAITING)
            .Select(t => (double?)t.EstimatedWaitMinutes).AverageAsync(ct) ?? 0;
        return new ServiceManagementSummaryDto(total, active, tokensToday, avgWait);
    }

    public async Task<IReadOnlyList<AdminServiceDto>> GetServicesAsync(CancellationToken ct = default)
    {
        var today = DateTime.Today;
        var services = await _db.Services.AsNoTracking().OrderBy(s => s.DisplayOrder).ToListAsync(ct);
        var result = new List<AdminServiceDto>();

        foreach (var s in services)
        {
            var volume = await _db.Tokens.CountAsync(t => t.ServiceId == s.Id && t.CreatedAt >= today, ct);
            var avgWait = await _db.Tokens.Where(t => t.ServiceId == s.Id && t.Status == TokenStatus.WAITING)
                .Select(t => (double?)(t.CalledAt ?? DateTime.Now).Subtract(t.CreatedAt).TotalMinutes)
                .AverageAsync(ct);
            var waitStr = avgWait.HasValue ? $"{(int)avgWait.Value:D2}m {(int)((avgWait.Value % 1) * 60):D2}s" : "--:--";

            result.Add(new AdminServiceDto(
                s.Id, s.Code, s.Name, s.Description, s.Icon, s.DisplayOrder, s.IsActive,
                GetDepartment(s.Code), s.IsActive ? "Operational" : "Maintenance",
                volume, waitStr));
        }
        return result;
    }

    public async Task<AdminServiceDto> CreateServiceAsync(UpsertServiceRequest request, CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var entity = new Service
        {
            Code = request.Code, Name = request.Name, Description = request.Description,
            Icon = request.Icon, DisplayOrder = request.DisplayOrder, IsActive = request.IsActive,
            CreatedAt = now, UpdatedAt = now
        };
        _db.Services.Add(entity);
        await _db.SaveChangesAsync(ct);
        return (await GetServicesAsync(ct)).First(s => s.Id == entity.Id);
    }

    public async Task<AdminServiceDto?> UpdateServiceAsync(int id, UpsertServiceRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Services.FindAsync([id], ct);
        if (entity == null) return null;
        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Icon = request.Icon;
        entity.DisplayOrder = request.DisplayOrder;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);
        return (await GetServicesAsync(ct)).FirstOrDefault(s => s.Id == id);
    }

    public async Task<IReadOnlyList<AdminSubServiceDto>> GetSubServicesAsync(CancellationToken ct = default)
    {
        return await _db.SubServices.AsNoTracking()
            .Include(s => s.Service)
            .OrderBy(s => s.ServiceId).ThenBy(s => s.DisplayOrder)
            .Select(s => new AdminSubServiceDto(
                s.Id, s.ServiceId, s.Service.Name, s.Code, s.Name,
                s.TokenPrefix, s.EstimatedServiceMinutes, s.IsActive))
            .ToListAsync(ct);
    }

    public async Task<AdminSubServiceDto> CreateSubServiceAsync(UpsertSubServiceRequest request, CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var entity = new SubService
        {
            ServiceId = request.ServiceId, Code = request.Code, Name = request.Name,
            Description = request.Description, TokenPrefix = request.TokenPrefix,
            Icon = request.Icon, EstimatedServiceMinutes = request.EstimatedServiceMinutes,
            DisplayOrder = request.DisplayOrder, IsActive = request.IsActive,
            CreatedAt = now, UpdatedAt = now
        };
        _db.SubServices.Add(entity);
        await _db.SaveChangesAsync(ct);
        return (await GetSubServicesAsync(ct)).First(s => s.Id == entity.Id);
    }

    public async Task<AdminSubServiceDto?> UpdateSubServiceAsync(int id, UpsertSubServiceRequest request, CancellationToken ct = default)
    {
        var entity = await _db.SubServices.FindAsync([id], ct);
        if (entity == null) return null;
        entity.ServiceId = request.ServiceId;
        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.TokenPrefix = request.TokenPrefix;
        entity.Icon = request.Icon;
        entity.EstimatedServiceMinutes = request.EstimatedServiceMinutes;
        entity.DisplayOrder = request.DisplayOrder;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);
        return (await GetSubServicesAsync(ct)).FirstOrDefault(s => s.Id == id);
    }

    public async Task<IReadOnlyList<AdminCounterDto>> GetCountersAsync(CancellationToken ct = default)
    {
        var counters = await _db.Counters.AsNoTracking()
            .Include(c => c.ServiceAssignments).ThenInclude(a => a.Service)
            .Include(c => c.StaffUsers)
            .OrderBy(c => c.CounterNo)
            .ToListAsync(ct);

        return counters.Select(c => new AdminCounterDto(
            c.Id, c.CounterNo, c.CounterName, c.Status.ToString(), c.IsActive,
            c.StaffUsers.FirstOrDefault(s => s.IsActive)?.FullName,
            c.ServiceAssignments.Where(a => a.IsActive).Select(a => a.Service.Name).ToList()
        )).ToList();
    }

    public async Task<AdminCounterDto> CreateCounterAsync(UpsertCounterRequest request, CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var counter = new Counter
        {
            CounterNo = request.CounterNo, CounterName = request.CounterName,
            Status = Enum.Parse<CounterStatus>(request.Status, true),
            IsActive = request.IsActive, CreatedAt = now, UpdatedAt = now
        };
        _db.Counters.Add(counter);
        await _db.SaveChangesAsync(ct);
        if (request.ServiceIds != null)
        {
            foreach (var sid in request.ServiceIds)
                _db.CounterServiceAssignments.Add(new CounterServiceAssignment { CounterId = counter.Id, ServiceId = sid, IsActive = true });
            await _db.SaveChangesAsync(ct);
        }
        return (await GetCountersAsync(ct)).First(c => c.Id == counter.Id);
    }

    public async Task<AdminCounterDto?> UpdateCounterAsync(int id, UpsertCounterRequest request, CancellationToken ct = default)
    {
        var counter = await _db.Counters.FindAsync([id], ct);
        if (counter == null) return null;
        counter.CounterNo = request.CounterNo;
        counter.CounterName = request.CounterName;
        counter.Status = Enum.Parse<CounterStatus>(request.Status, true);
        counter.IsActive = request.IsActive;
        counter.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);
        return (await GetCountersAsync(ct)).FirstOrDefault(c => c.Id == id);
    }

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

    public async Task<TokenHistoryReportDto> GetTokenHistoryReportAsync(TokenHistoryFilter filter, CancellationToken ct = default)
    {
        var query = _db.Tokens.AsNoTracking()
            .Include(t => t.Service).Include(t => t.SubService).Include(t => t.Counter)
            .AsQueryable();

        if (filter.From.HasValue) query = query.Where(t => t.CreatedAt >= filter.From);
        if (filter.To.HasValue) query = query.Where(t => t.CreatedAt <= filter.To);
        if (filter.ServiceId.HasValue) query = query.Where(t => t.ServiceId == filter.ServiceId);
        if (filter.CounterId.HasValue) query = query.Where(t => t.CounterId == filter.CounterId);
        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<TokenStatus>(filter.Status, true, out var st))
            query = query.Where(t => t.Status == st);

        var tokens = await query.OrderByDescending(t => t.CreatedAt).Take(500).ToListAsync(ct);
        var total = tokens.Count;
        var completed = tokens.Count(t => t.Status == TokenStatus.COMPLETED);
        var avgWait = tokens.Where(t => t.CalledAt.HasValue)
            .Select(t => (t.CalledAt!.Value - t.CreatedAt).TotalMinutes)
            .DefaultIfEmpty(0).Average();

        var rows = tokens.Select(t => new TokenHistoryRowDto(
            t.Id, t.TokenNo, t.Service.Name, t.SubService.Name,
            t.Counter?.CounterName, t.CreatedAt, t.CalledAt, t.CompletedAt, t.Status.ToString())).ToList();

        return new TokenHistoryReportDto(total, completed, avgWait, rows);
    }

    private static string GetDepartment(string code) => code switch
    {
        "CASH" => "Front Office",
        "ACC" => "Front Office",
        "LOAN" => "Executive Wings",
        "CARD" => "Front Office",
        _ => "Customer Service"
    };
}
