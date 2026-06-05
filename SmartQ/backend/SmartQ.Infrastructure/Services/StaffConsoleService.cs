using Microsoft.EntityFrameworkCore;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;
using SmartQ.Domain.Entities;
using SmartQ.Domain.Enums;
using SmartQ.Infrastructure.Persistence;

namespace SmartQ.Infrastructure.Services;

public class StaffConsoleService : IStaffConsoleService
{
    private readonly SmartQDbContext _db;
    private readonly IQueueNotificationService _notify;

    public StaffConsoleService(SmartQDbContext db, IQueueNotificationService notify)
    {
        _db = db;
        _notify = notify;
    }

    public async Task<StaffConsoleContextDto> GetContextAsync(int counterId, CancellationToken ct = default)
    {
        var branchName = await GetSettingAsync("BRANCH_NAME", "Branch Alpha", ct);
        var counter = await _db.Counters.AsNoTracking().FirstAsync(c => c.Id == counterId, ct);
        var staff = await _db.StaffUsers.AsNoTracking().Where(s => s.CounterId == counterId && s.IsActive).OrderBy(s => s.Id).FirstOrDefaultAsync(ct);
        var assigned = await _db.CounterServiceAssignments.AsNoTracking()
            .Where(a => a.CounterId == counterId && a.IsActive && a.Service.IsActive)
            .Select(a => new { a.ServiceId, a.Service.Code, a.Service.Name, a.Service.Icon })
            .Distinct()
            .ToListAsync(ct);

        var prefixes = await _db.SubServices.AsNoTracking()
            .Where(s => s.IsActive && assigned.Select(a => a.ServiceId).Contains(s.ServiceId))
            .GroupBy(s => s.ServiceId)
            .Select(g => new { ServiceId = g.Key, Prefixes = g.Select(x => x.TokenPrefix).ToList() })
            .ToListAsync(ct);

        var mapped = assigned.Select(a => new StaffAssignedServiceDto(
            a.ServiceId,
            a.Code,
            a.Name,
            string.IsNullOrWhiteSpace(a.Icon) ? "category" : a.Icon,
            prefixes.FirstOrDefault(p => p.ServiceId == a.ServiceId)?.Prefixes ?? new List<string>()))
            .ToList();

        var displayMessages = await _db.DisplayMessages.AsNoTracking()
            .Where(x => x.IsActive && x.MessageKey.StartsWith("STAFF_"))
            .ToDictionaryAsync(x => x.MessageKey, x => x.MessageText, ct);

        return new StaffConsoleContextDto(
            new StaffCounterContextDto(counter.Id, counter.CounterNo, counter.CounterName, counter.Status.ToString(), branchName),
            staff == null ? null : new StaffUserContextDto(staff.Id, staff.FullName, staff.Role.ToString()),
            mapped,
            await GetSettingBoolAsync("SYSTEM_ONLINE", true, ct),
            await GetSettingBoolAsync("CALL_NEXT_LOCK_WHEN_ACTIVE_TOKEN", true, ct),
            displayMessages,
            DateTime.Now);
    }

    public async Task<StaffConsoleSummaryV2Dto> GetSummaryAsync(int counterId, CancellationToken ct = default)
    {
        var todayStart = DateTime.Today;
        var nextDay = todayStart.AddDays(1);
        var serviceIds = await GetAssignedServiceIdsAsync(counterId, ct);
        var waitingQuery = _db.Tokens.AsNoTracking().Where(t => t.Status == TokenStatus.WAITING);
        if (serviceIds.Count > 0) waitingQuery = waitingQuery.Where(t => serviceIds.Contains(t.ServiceId));

        var waiting = await waitingQuery.CountAsync(ct);
        var completed = await _db.Tokens.AsNoTracking()
            .CountAsync(t => t.CounterId == counterId && t.CompletedAt >= todayStart && t.CompletedAt < nextDay, ct);
        var skipped = await _db.Tokens.AsNoTracking()
            .CountAsync(t => t.CounterId == counterId && t.SkippedAt >= todayStart && t.SkippedAt < nextDay, ct);
        var served = completed + skipped;

        var avgWait = (int)Math.Round(await waitingQuery.Select(t => (double?)EF.Functions.DateDiffMinute(t.CreatedAt, DateTime.Now)).AverageAsync(ct) ?? 0d);
        var avgServiceSeconds = await _db.Tokens.AsNoTracking()
            .Where(t => t.CounterId == counterId && t.CompletedAt >= todayStart && t.CompletedAt < nextDay && t.StartedAt.HasValue)
            .Select(t => (double?)EF.Functions.DateDiffSecond(t.StartedAt!.Value, t.CompletedAt!.Value))
            .AverageAsync(ct) ?? 0d;
        var pressure = waiting >= 15 ? "HIGH" : waiting >= 7 ? "NORMAL" : "LOW";
        var status = await _db.Counters.AsNoTracking().Where(c => c.Id == counterId).Select(c => c.Status.ToString()).FirstOrDefaultAsync(ct) ?? "AVAILABLE";

        return new StaffConsoleSummaryV2Dto(
            waiting,
            served,
            completed,
            skipped,
            avgWait,
            FormatDuration((int)avgServiceSeconds),
            status,
            pressure);
    }

    public async Task<StaffActiveSessionDto?> GetActiveSessionAsync(int counterId, CancellationToken ct = default)
    {
        var token = await _db.Tokens.AsNoTracking()
            .Include(t => t.Service)
            .Include(t => t.SubService)
            .Where(t => t.CounterId == counterId && (t.Status == TokenStatus.CALLED || t.Status == TokenStatus.SERVING))
            .OrderByDescending(t => t.StartedAt ?? t.CalledAt)
            .FirstOrDefaultAsync(ct);
        return token == null ? null : MapActiveSession(token);
    }

    public async Task<IReadOnlyList<StaffQueueItemDto>> GetQueueAsync(int counterId, string scope, CancellationToken ct = default)
    {
        var query = _db.Tokens.AsNoTracking()
            .Include(t => t.Service)
            .Include(t => t.SubService)
            .Where(t => t.Status == TokenStatus.WAITING);

        if (!string.Equals(scope, "all-branch", StringComparison.OrdinalIgnoreCase))
        {
            var serviceIds = await GetAssignedServiceIdsAsync(counterId, ct);
            query = query.Where(t => serviceIds.Contains(t.ServiceId));
        }

        var tokens = await query.OrderBy(t => t.QueuedAt ?? t.CreatedAt).ThenBy(t => t.CreatedAt).Take(150).ToListAsync(ct);
        return tokens.Select((t, idx) => new StaffQueueItemDto(
            t.Id, FormatStaffTokenNo(t), t.Service.Name, t.SubService.Name,
            Math.Max(0, (int)(DateTime.Now - t.CreatedAt).TotalMinutes),
            t.Priority.ToString(), t.Status.ToString(), t.CreatedAt, idx + 1)).ToList();
    }

    public async Task<CallNextActionResultDto> CallNextAsync(int counterId, int? staffUserId = null, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        var lockEnabled = await GetSettingBoolAsync("CALL_NEXT_LOCK_WHEN_ACTIVE_TOKEN", true, ct);
        if (lockEnabled)
        {
            var hasActive = await _db.Tokens.AnyAsync(t => t.CounterId == counterId && (t.Status == TokenStatus.CALLED || t.Status == TokenStatus.SERVING), ct);
            if (hasActive)
            {
                var lockMessage = await GetDisplayMessageAsync("STAFF_CALL_NEXT_LOCKED_MESSAGE", "Complete or skip current token before calling next", ct);
                return new CallNextActionResultDto(false, lockMessage, null);
            }
        }

        var serviceIds = await GetAssignedServiceIdsAsync(counterId, ct);
        if (serviceIds.Count == 0) return new CallNextActionResultDto(false, "No services assigned to this counter.", null);

        var priorityEnabled = await GetSettingBoolAsync("ENABLE_PRIORITY_QUEUE", true, ct);
        IQueryable<Token> query = _db.Tokens.Include(t => t.Service).Include(t => t.SubService)
            .Where(t => t.Status == TokenStatus.WAITING && serviceIds.Contains(t.ServiceId));
        query = priorityEnabled
            ? query.OrderByDescending(t => t.Priority != TokenPriority.STANDARD)
                .ThenBy(t => t.QueuedAt ?? t.CreatedAt)
                .ThenBy(t => t.CreatedAt)
            : query.OrderBy(t => t.QueuedAt ?? t.CreatedAt)
                .ThenBy(t => t.CreatedAt);

        // Keep read + write in same transaction to minimize race risk on concurrent call-next requests.
        var token = await query.FirstOrDefaultAsync(ct);
        if (token == null) return new CallNextActionResultDto(false, "No waiting tokens available for this counter's services.", null);

        var oldStatus = token.Status;
        token.Status = TokenStatus.CALLED;
        token.CounterId = counterId;
        token.CalledAt = DateTime.Now;
        var counter = await _db.Counters.FirstAsync(c => c.Id == counterId, ct);
        counter.Status = CounterStatus.SERVING;
        _db.TokenStatusHistories.Add(new TokenStatusHistory
        {
            TokenId = token.Id,
            OldStatus = oldStatus,
            NewStatus = TokenStatus.CALLED,
            CounterId = counterId,
            StaffUserId = staffUserId,
            ChangedAt = DateTime.Now,
            Remarks = "Called next"
        });
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var dto = MapActiveSession(token);
        await _notify.TokenCalledAsync(dto, ct);
        await _notify.QueueUpdatedAsync(new { counterId }, ct);
        await _notify.DisplayUpdatedAsync(await new DisplayService(_db).GetDisplayBoardAsync(ct), ct);
        return new CallNextActionResultDto(true, "Token called successfully.", dto);
    }

    public Task<TokenActionResultDto?> RecallAsync(int tokenId, int counterId, int? staffUserId = null, CancellationToken ct = default) =>
        UpdateStatusAsync(tokenId, counterId, staffUserId, TokenStatus.CALLED, [TokenStatus.CALLED, TokenStatus.SERVING], "TokenRecalled", "Token recalled", ct);

    public Task<TokenActionResultDto?> StartServiceAsync(int tokenId, int counterId, int? staffUserId = null, CancellationToken ct = default) =>
        UpdateStatusAsync(tokenId, counterId, staffUserId, TokenStatus.SERVING, [TokenStatus.CALLED], "TokenStarted", "Service started", ct, setStartedAt: true);

    public Task<TokenActionResultDto?> CompleteAsync(int tokenId, int counterId, int? staffUserId = null, CancellationToken ct = default) =>
        UpdateStatusAsync(tokenId, counterId, staffUserId, TokenStatus.COMPLETED, [TokenStatus.CALLED, TokenStatus.SERVING], "TokenCompleted", "Service completed", ct, setCompletedAt: true);

    public Task<TokenActionResultDto?> NoShowAsync(int tokenId, int counterId, int? staffUserId = null, CancellationToken ct = default) =>
        UpdateStatusAsync(tokenId, counterId, staffUserId, TokenStatus.SKIPPED, [TokenStatus.CALLED], "TokenSkipped", "No show", ct, setSkippedAt: true, remarks: "No show");

    public async Task<TokenActionResultDto?> CancelAsync(int tokenId, int counterId, int? staffUserId = null, CancellationToken ct = default)
    {
        var token = await _db.Tokens.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tokenId, ct);
        if (token == null) return null;

        if (token.Status is TokenStatus.WAITING)
        {
            var serviceIds = await GetAssignedServiceIdsAsync(counterId, ct);
            if (!serviceIds.Contains(token.ServiceId))
                return new TokenActionResultDto(false, "Token is not in this counter's assigned services.", null);
            return await UpdateStatusAsync(tokenId, counterId, staffUserId, TokenStatus.CANCELLED,
                [TokenStatus.WAITING], "TokenCancelled", "Token cancelled", ct, setCancelledAt: true, remarks: "Cancelled by staff", allowUnassignedCounter: true);
        }

        return await UpdateStatusAsync(tokenId, counterId, staffUserId, TokenStatus.CANCELLED,
            [TokenStatus.CALLED, TokenStatus.SERVING], "TokenCancelled", "Token cancelled", ct, setCancelledAt: true, remarks: "Cancelled by staff");
    }

    public async Task<TokenActionResultDto?> TransferAsync(int tokenId, StaffTransferTokenRequest request, int counterId, int? staffUserId = null, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        var token = await _db.Tokens.Include(t => t.Service).Include(t => t.SubService).FirstOrDefaultAsync(t => t.Id == tokenId, ct);
        if (token == null) return null;
        if (token.CounterId != counterId && !await IsAdminAsync(staffUserId, ct)) return new TokenActionResultDto(false, "Unauthorized for this token.", null);

        var oldStatus = token.Status;
        var oldTokenNo = token.TokenNo;
        var oldSubServiceName = token.SubService.Name;
        token.ServiceId = request.TargetServiceId;
        token.SubServiceId = request.TargetSubServiceId;
        token.Status = TokenStatus.WAITING;
        token.CounterId = request.TargetCounterId;
        token.CalledAt = null;
        token.StartedAt = null;
        token.QueuedAt = DateTime.Now;
        token.TransferCount += 1;
        token.LastTransferredAt = DateTime.Now;
        token.TransferredFromTokenNo = token.TransferCount > 1 && !string.IsNullOrWhiteSpace(token.TransferredFromTokenNo)
            ? token.TransferredFromTokenNo
            : oldTokenNo;

        // Always align code with destination sub-service on transfer.
        var sub = await _db.SubServices.AsNoTracking().FirstAsync(s => s.Id == request.TargetSubServiceId, ct);
        var (nextSequenceNo, nextTokenNo) = await GetNextTokenForSubServiceAsync(sub.Id, sub.TokenPrefix, ct);
        token.TokenPrefix = sub.TokenPrefix;
        token.SequenceNo = nextSequenceNo;
        token.SequenceDate = DateOnly.FromDateTime(DateTime.Today);
        token.TokenNo = nextTokenNo;
        var transferFlow = $"Transfer: {oldSubServiceName} -> {sub.Name}";

        _db.TokenStatusHistories.Add(new TokenStatusHistory
        {
            TokenId = token.Id,
            OldStatus = oldStatus,
            NewStatus = TokenStatus.TRANSFERRED,
            CounterId = counterId,
            StaffUserId = staffUserId,
            ChangedAt = DateTime.Now,
            Remarks = transferFlow
        });
        _db.TokenStatusHistories.Add(new TokenStatusHistory
        {
            TokenId = token.Id,
            OldStatus = TokenStatus.TRANSFERRED,
            NewStatus = TokenStatus.WAITING,
            CounterId = request.TargetCounterId,
            StaffUserId = staffUserId,
            ChangedAt = DateTime.Now,
            Remarks = $"{transferFlow} | queued"
        });
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        await _notify.QueueUpdatedAsync(new { counterId, tokenId }, ct);
        await _notify.DisplayUpdatedAsync(await new DisplayService(_db).GetDisplayBoardAsync(ct), ct);
        return new TokenActionResultDto(true, "Token transferred successfully.", await GetActiveSessionAsync(counterId, ct));
    }

    public async Task<IReadOnlyList<StaffTokenHistoryItemDto>> GetTokenHistoryAsync(
        int counterId,
        DateTime? date,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? status,
        int? serviceId,
        CancellationToken ct = default)
    {
        DateTime periodStart;
        DateTime periodEnd;
        if (dateFrom.HasValue && dateTo.HasValue)
        {
            periodStart = dateFrom.Value.Date;
            periodEnd = dateTo.Value.Date.AddDays(1);
            if (periodEnd <= periodStart)
                periodEnd = periodStart.AddDays(1);
        }
        else
        {
            var day = date?.Date ?? DateTime.Today;
            periodStart = day;
            periodEnd = day.AddDays(1);
        }

        var query = _db.Tokens.AsNoTracking().Include(t => t.Service)
            .Where(t => t.CounterId == counterId && t.CreatedAt >= periodStart && t.CreatedAt < periodEnd);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TokenStatus>(status, true, out var st)) query = query.Where(t => t.Status == st);
        if (serviceId.HasValue) query = query.Where(t => t.ServiceId == serviceId.Value);

        return await query.OrderByDescending(t => t.CreatedAt)
            .Select(t => new StaffTokenHistoryItemDto(
                t.Id,
                t.TokenNo,
                t.Service.Name,
                t.CalledAt,
                t.CompletedAt.HasValue && t.StartedAt.HasValue ? FormatDuration(EF.Functions.DateDiffSecond(t.StartedAt.Value, t.CompletedAt.Value)) : "--",
                t.Status.ToString()))
            .ToListAsync(ct);
    }

    public async Task<StaffPerformanceDto> GetPerformanceAsync(int? staffUserId, int counterId, string range, CancellationToken ct = default)
    {
        var isWeek = range.Equals("week", StringComparison.OrdinalIgnoreCase);
        var periodStart = isWeek ? DateTime.Today.AddDays(-6) : DateTime.Today;
        var periodEnd = DateTime.Today.AddDays(1);
        var prevStart = isWeek ? DateTime.Today.AddDays(-13) : DateTime.Today.AddDays(-1);
        var prevEnd = isWeek ? DateTime.Today.AddDays(-6) : DateTime.Today;

        var staffName = staffUserId.HasValue
            ? await _db.StaffUsers.AsNoTracking().Where(s => s.Id == staffUserId.Value).Select(s => s.FullName).FirstOrDefaultAsync(ct)
            : await _db.StaffUsers.AsNoTracking().Where(s => s.CounterId == counterId && s.IsActive).OrderBy(s => s.Id).Select(s => s.FullName).FirstOrDefaultAsync(ct);
        staffName ??= "Staff";

        var completed = await _db.Tokens.AsNoTracking()
            .Include(t => t.Service)
            .Include(t => t.SubService)
            .Where(t => t.CounterId == counterId && t.CompletedAt >= periodStart && t.CompletedAt < periodEnd)
            .ToListAsync(ct);

        var skippedCount = await _db.Tokens.AsNoTracking()
            .CountAsync(t => t.CounterId == counterId && t.SkippedAt >= periodStart && t.SkippedAt < periodEnd, ct);

        var prevCompleted = await _db.Tokens.AsNoTracking()
            .CountAsync(t => t.CounterId == counterId && t.CompletedAt >= prevStart && t.CompletedAt < prevEnd, ct);

        var prevSkipped = await _db.Tokens.AsNoTracking()
            .CountAsync(t => t.CounterId == counterId && t.SkippedAt >= prevStart && t.SkippedAt < prevEnd, ct);

        var serviceSeconds = completed
            .Where(t => t.StartedAt.HasValue && t.CompletedAt.HasValue)
            .Select(t => (int)(t.CompletedAt!.Value - t.StartedAt!.Value).TotalSeconds)
            .ToList();
        var avgSeconds = serviceSeconds.Count == 0 ? 0 : serviceSeconds.Average();

        var prevServiceSeconds = await _db.Tokens.AsNoTracking()
            .Where(t => t.CounterId == counterId && t.CompletedAt >= prevStart && t.CompletedAt < prevEnd && t.StartedAt.HasValue && t.CompletedAt.HasValue)
            .Select(t => EF.Functions.DateDiffSecond(t.StartedAt!.Value, t.CompletedAt!.Value))
            .ToListAsync(ct);
        var prevAvgSeconds = prevServiceSeconds.Count == 0 ? 0 : prevServiceSeconds.Average();

        var servedCount = completed.Count;
        var totalProcessed = servedCount + skippedCount;
        var rate = totalProcessed == 0 ? 0 : Math.Round((decimal)servedCount / totalProcessed * 100, 2);

        var prevTotal = prevCompleted + prevSkipped;
        var prevRate = prevTotal == 0 ? 0 : Math.Round((decimal)prevCompleted / prevTotal * 100, 2);

        var servedTrend = BuildServedTrendLabel(servedCount, prevCompleted);
        var avgServiceTrend = BuildAvgServiceTrendLabel((int)avgSeconds, (int)prevAvgSeconds);
        var completionTrend = Math.Abs(rate - prevRate) <= 2 ? "Stable" : FormatSignedPercent(rate - prevRate);

        var dailyTarget = await GetSettingIntAsync("STAFF_DAILY_TARGET", 50, ct);
        var servedProgress = dailyTarget == 0 ? 0 : Math.Min(100, Math.Round((decimal)servedCount / dailyTarget * 100, 0));

        var branchServiceSeconds = await _db.Tokens.AsNoTracking()
            .Where(t => t.CompletedAt >= periodStart && t.CompletedAt < periodEnd && t.StartedAt.HasValue && t.CompletedAt.HasValue)
            .Select(t => EF.Functions.DateDiffSecond(t.StartedAt!.Value, t.CompletedAt!.Value))
            .ToListAsync(ct);
        var branchAvgSeconds = branchServiceSeconds.Count == 0 ? 0 : branchServiceSeconds.Average();

        var avgServiceHint = avgSeconds <= 0
            ? "No completed services yet"
            : avgSeconds <= branchAvgSeconds * 0.95
                ? "Top 5% efficiency in branch"
                : avgSeconds <= branchAvgSeconds
                    ? "Above branch average"
                    : "Room to improve vs branch average";

        var avgServiceProgress = avgSeconds <= 0 || branchAvgSeconds <= 0
            ? 0
            : Math.Min(100, Math.Max(10, (decimal)(branchAvgSeconds / avgSeconds * 85)));

        var activityTokens = await _db.Tokens.AsNoTracking()
            .Include(t => t.Service)
            .Include(t => t.SubService)
            .Where(t => t.CounterId == counterId && (
                (t.CreatedAt >= periodStart && t.CreatedAt < periodEnd) ||
                (t.CalledAt >= periodStart && t.CalledAt < periodEnd) ||
                (t.StartedAt >= periodStart && t.StartedAt < periodEnd) ||
                (t.CompletedAt >= periodStart && t.CompletedAt < periodEnd) ||
                (t.SkippedAt >= periodStart && t.SkippedAt < periodEnd) ||
                ((t.Status == TokenStatus.CALLED || t.Status == TokenStatus.SERVING) &&
                 t.CalledAt >= periodStart && t.CalledAt < periodEnd)))
            .ToListAsync(ct);

        var hourlyTraffic = BuildHourlyTraffic(activityTokens);
        var hourly = hourlyTraffic
            .Select(h => new HourlyServedPointDto(h.HourLabel, h.CashCount + h.AccountCount + h.LoanCount))
            .ToList();

        var timeline = await BuildPerformanceTimelineAsync(counterId, periodStart, periodEnd, activityTokens, ct);

        var tip = await BuildPerformanceTipAsync(completed, hourlyTraffic, ct);

        var reportDateLabel = isWeek
            ? $"{periodStart:MMMM d} – {DateTime.Today:MMMM d, yyyy}"
            : DateTime.Today.ToString("MMMM d, yyyy");
        var rangeLabel = isWeek ? "This Week" : "Today";
        var servedLabel = isWeek ? "Served This Week" : "Served Today";

        return new StaffPerformanceDto(
            servedCount,
            FormatDuration((int)avgSeconds),
            skippedCount,
            rate,
            hourly,
            timeline,
            tip,
            staffName,
            reportDateLabel,
            rangeLabel,
            servedLabel,
            dailyTarget,
            servedProgress,
            servedTrend,
            avgServiceTrend,
            avgServiceProgress,
            avgServiceHint,
            completionTrend,
            rate,
            "Goal: >90% minimum",
            hourlyTraffic);
    }

    public async Task<StaffNotificationResponseDto> GetNotificationsAsync(int counterId, CancellationToken ct = default)
    {
        var warningMinutes = await GetSettingIntAsync("WAIT_TIME_WARNING_MINUTES", 10, ct);
        var serviceIds = await GetAssignedServiceIdsAsync(counterId, ct);
        var items = new List<StaffNotificationItemDto>();
        var nextEligible = await _db.Tokens.AsNoTracking()
            .Where(t => t.Status == TokenStatus.WAITING && serviceIds.Contains(t.ServiceId))
            .OrderBy(t => t.QueuedAt ?? t.CreatedAt)
            .ThenBy(t => t.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (nextEligible != null) items.Add(new StaffNotificationItemDto("QUEUE", "New eligible token waiting", $"Next token {nextEligible.TokenNo} is ready for call.", DateTime.Now, true));
        var thresholdHit = await _db.Tokens.AsNoTracking().AnyAsync(t => t.Status == TokenStatus.WAITING && EF.Functions.DateDiffMinute(t.CreatedAt, DateTime.Now) >= warningMinutes, ct);
        if (thresholdHit) items.Add(new StaffNotificationItemDto("WARNING", "Queue waiting time exceeded", $"At least one token exceeded {warningMinutes} minutes.", DateTime.Now, true));
        var overloaded = await _db.Counters.AsNoTracking().AnyAsync(c => c.Status == CounterStatus.SERVING && c.Tokens.Count(t => t.Status == TokenStatus.WAITING) > 10, ct);
        if (overloaded) items.Add(new StaffNotificationItemDto("LOAD", "Counter overload detected", "Some counters have high waiting load.", DateTime.Now, false));
        items.Add(new StaffNotificationItemDto("SYSTEM", "System status", "Queue system is online.", DateTime.Now, false));
        return new StaffNotificationResponseDto(items.Count(i => i.IsNew), items);
    }

    public async Task<StaffDashboardDto> GetDashboardAsync(int counterId, CancellationToken ct = default)
    {
        var context = await GetContextAsync(counterId, ct);
        var summary = await GetSummaryAsync(counterId, ct);
        var notifications = await GetNotificationsAsync(counterId, ct);
        var todayStart = DateTime.Today;
        var nextDay = todayStart.AddDays(1);

        var tokensToday = await _db.Tokens.AsNoTracking()
            .CountAsync(t => t.CreatedAt >= todayStart && t.CreatedAt < nextDay, ct);

        var totalProcessed = summary.CompletedToday + summary.SkippedToday;
        var efficiency = totalProcessed == 0 ? 0 : (int)Math.Round((double)summary.CompletedToday / totalProcessed * 100);

        var activeCounters = await _db.Counters.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CounterNo)
            .Select(c => new
            {
                c.Id,
                c.CounterName,
                c.Status,
                StaffName = c.StaffUsers.Where(s => s.IsActive).OrderBy(s => s.Id).Select(s => s.FullName).FirstOrDefault(),
                CurrentToken = c.Tokens
                    .Where(t => t.Status == TokenStatus.CALLED || t.Status == TokenStatus.SERVING)
                    .OrderByDescending(t => t.CalledAt)
                    .Select(t => t.TokenNo)
                    .FirstOrDefault(),
                Waiting = c.Tokens.Count(t => t.Status == TokenStatus.WAITING)
            })
            .Take(8)
            .ToListAsync(ct);

        var maxWaiting = Math.Max(1, activeCounters.Select(c => c.Waiting).DefaultIfEmpty(1).Max());
        var countersDto = activeCounters.Select(c =>
        {
            var pct = (int)Math.Min(100, Math.Round((double)c.Waiting / maxWaiting * 100));
            var label = pct >= 80 ? "High" : pct >= 40 ? "Medium" : "Low";
            return new StaffDashboardCounterDto(c.Id, c.CounterName, c.StaffName, c.Status.ToString(), c.CurrentToken, pct, label);
        }).ToList();

        var queueComposition = await _db.Tokens.AsNoTracking()
            .Where(t => t.Status == TokenStatus.WAITING)
            .GroupBy(t => new { t.Service.Code, t.Service.Name })
            .Select(g => new StaffDashboardCompositionDto(g.Key.Code, g.Key.Name, g.Count()))
            .OrderByDescending(x => x.WaitingCount)
            .Take(4)
            .ToListAsync(ct);

        var systemStatuses = new List<StaffSystemStatusItemDto>();
        var serverMs = await GetSettingAsync("MAIN_SERVER_RESPONSE_MS", "", ct);
        if (!string.IsNullOrWhiteSpace(serverMs))
            systemStatuses.Add(new StaffSystemStatusItemDto("MAIN_SERVER", "Main Server", $"Response time: {serverMs}ms", "OK"));

        var printerLevel = await GetSettingAsync("TOKEN_PRINTER_PAPER_LEVEL", "", ct);
        if (!string.IsNullOrWhiteSpace(printerLevel))
            systemStatuses.Add(new StaffSystemStatusItemDto("TOKEN_PRINTER", "Token Printer", $"Paper level: {printerLevel}%", "OK"));

        var lastTokenAt = await _db.Tokens.AsNoTracking().OrderByDescending(t => t.CreatedAt).Select(t => t.CreatedAt).FirstOrDefaultAsync(ct);
        if (lastTokenAt != default)
        {
            var minsAgo = Math.Max(0, (int)(DateTime.Now - lastTokenAt).TotalMinutes);
            systemStatuses.Add(new StaffSystemStatusItemDto("DATA_SYNC", "Data Sync", $"Last sync: {minsAgo} min ago", "INFO"));
        }

        var streamTitle = await GetSettingAsync("STAFF_LIVE_STREAM_TITLE", "Zone A", ct);
        var streamUrl = await GetSettingAsync("STAFF_LIVE_STREAM_URL", "", ct);

        return new StaffDashboardDto(
            context.Counter.BranchName,
            $"{context.Counter.BranchName} Monitor",
            "Real-time performance analytics for current shift.",
            context.SystemOnline ? 100 : 0,
            $"{summary.AvgWaitMinutes:00}:{Math.Abs(DateTime.Now.Second % 60):00}",
            tokensToday,
            efficiency,
            countersDto,
            queueComposition,
            systemStatuses,
            notifications,
            streamTitle,
            streamUrl,
            DateTime.Now);
    }

    public async Task<StaffTokenDetailsDto?> GetTokenDetailsAsync(int tokenId, CancellationToken ct = default)
    {
        var token = await _db.Tokens.AsNoTracking()
            .Include(t => t.Service).Include(t => t.SubService).Include(t => t.Language).Include(t => t.Counter)
            .FirstOrDefaultAsync(t => t.Id == tokenId, ct);
        if (token == null) return null;

        var queuePosition = await _db.Tokens.AsNoTracking()
            .CountAsync(t =>
                t.Status == TokenStatus.WAITING
                && t.ServiceId == token.ServiceId
                && (t.QueuedAt ?? t.CreatedAt) <= (token.QueuedAt ?? token.CreatedAt), ct);

        var histories = await _db.TokenStatusHistories.AsNoTracking()
            .Where(h => h.TokenId == tokenId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync(ct);

        var counterIds = histories
            .Where(h => h.CounterId.HasValue)
            .Select(h => h.CounterId!.Value)
            .Distinct()
            .ToList();
        var counterNames = counterIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.Counters.AsNoTracking()
                .Where(c => counterIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CounterName, ct);

        var kioskLabel = await GetDisplayMessageAsync("STAFF_TOKEN_SOURCE_KIOSK", "Self-Service Kiosk", ct);
        var journey = histories.Select(h =>
        {
            var counterName = h.CounterId.HasValue && counterNames.TryGetValue(h.CounterId.Value, out var name) ? name : null;
            var mapped = MapJourneyStep(h.NewStatus, h.OldStatus, h.ChangedAt, h.Remarks, counterName, kioskLabel);
            return new TokenJourneyItemDto(h.NewStatus.ToString(), h.ChangedAt, h.Remarks, mapped.Title, mapped.Subtitle);
        }).ToList();

        if (token.Status == TokenStatus.WAITING && journey.Count == 1)
        {
            var queuedAt = token.QueuedAt ?? token.CreatedAt;
            journey.Add(new TokenJourneyItemDto(
                TokenStatus.WAITING.ToString(),
                queuedAt,
                "Entered waiting pool",
                "Enters Waiting Pool",
                $"Global Queue • {queuedAt:HH:mm:ss}"));
        }

        var isPriority = token.Priority != TokenPriority.STANDARD;
        var customerName = await ResolveCustomerLabelAsync(token.Priority, ct);
        var customerSubtitleKey = isPriority ? "STAFF_CUSTOMER_SUBTITLE_VIP" : "STAFF_CUSTOMER_SUBTITLE_STANDARD";
        var customerSubtitleFallback = isPriority ? "Immediate priority handling" : "Standard queue customer";
        var customerSubtitle = await GetDisplayMessageAsync(customerSubtitleKey, customerSubtitleFallback, ct);

        return new StaffTokenDetailsDto(
            token.Id,
            FormatStaffTokenNo(token),
            token.Status.ToString(),
            token.Service.Name,
            token.SubService.Name,
            token.Language.Name,
            token.Priority.ToString(),
            token.CreatedAt,
            Math.Max(0, (int)(DateTime.Now - token.CreatedAt).TotalMinutes),
            Math.Max(1, queuePosition),
            customerName,
            customerSubtitle,
            journey);
    }

    public async Task<StaffTransferOptionsDto> GetTransferOptionsAsync(CancellationToken ct = default)
    {
        var services = await _db.Services.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.DisplayOrder)
            .Select(s => new StaffTransferOptionDto(s.Id, s.Code, s.Name)).ToListAsync(ct);
        var subServices = await _db.SubServices.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.DisplayOrder)
            .Select(s => new StaffTransferSubServiceDto(s.Id, s.ServiceId, s.Code, s.Name)).ToListAsync(ct);
        var counters = await _db.Counters.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.CounterNo)
            .Select(c => new StaffTransferCounterDto(c.Id, c.CounterNo, c.CounterName, c.Status.ToString())).ToListAsync(ct);
        return new StaffTransferOptionsDto(services, subServices, counters);
    }

    public async Task<StaffMyCounterDto> GetMyCounterAsync(int counterId, int? staffUserId = null, CancellationToken ct = default)
    {
        var context = await GetContextAsync(counterId, ct);
        var summary = await GetSummaryAsync(counterId, ct);
        var active = await GetActiveSessionAsync(counterId, ct);
        var queue = await GetQueueAsync(counterId, "my-services", ct);
        var performance = await GetPerformanceAsync(staffUserId, counterId, "today", ct);

        StaffMyCounterActiveDetailsDto? activeDetails = null;
        if (active != null)
        {
            var token = await _db.Tokens.AsNoTracking().FirstAsync(t => t.Id == active.TokenId, ct);
            var idFormat = await GetSettingAsync("STAFF_TOKEN_ID_FORMAT", "TK-{id}", ct);
            var customerLabel = await ResolveCustomerLabelAsync(token.Priority, ct);
            var waitMinutes = Math.Max(0, (int)(DateTime.Now - token.CreatedAt).TotalMinutes);
            activeDetails = new StaffMyCounterActiveDetailsDto(
                idFormat.Replace("{id}", token.Id.ToString(), StringComparison.OrdinalIgnoreCase),
                customerLabel,
                FormatWaitDuration(waitMinutes),
                waitMinutes);
        }

        var upcomingLimit = await GetSettingIntAsync("STAFF_MY_COUNTER_UPCOMING_COUNT", 3, ct);
        var upcoming = queue.Take(upcomingLimit).Select(q => new StaffMyCounterUpcomingTokenDto(
            q.TokenId,
            q.TokenNo,
            ExtractTokenPrefixBadge(q.TokenNo),
            q.SubServiceName,
            q.WaitMinutes)).ToList();

        var pressureThreshold = await GetSettingIntAsync("STAFF_QUEUE_PRESSURE_HIGH_THRESHOLD", 12, ct);
        var pressurePercent = pressureThreshold <= 0
            ? 0
            : (int)Math.Min(100, Math.Round((double)summary.Waiting / pressureThreshold * 100));
        var pressureLabel = summary.QueuePressure switch
        {
            "HIGH" => await GetDisplayMessageAsync("STAFF_QUEUE_PRESSURE_HIGH", "Queue pressure is high. Avoid taking breaks at this time.", ct),
            "NORMAL" => await GetDisplayMessageAsync("STAFF_QUEUE_PRESSURE_NORMAL", "Queue pressure is moderate.", ct),
            _ => await GetDisplayMessageAsync("STAFF_QUEUE_PRESSURE_LOW", "Queue pressure is low.", ct)
        };

        var efficiency = await BuildMyCounterEfficiencyAsync(performance, ct);
        return new StaffMyCounterDto(context, summary, active, activeDetails, upcoming, performance, efficiency, pressurePercent, pressureLabel);
    }

    public async Task<StaffCounterStatusResultDto> UpdateCounterStatusAsync(int counterId, string status, CancellationToken ct = default)
    {
        var counter = await _db.Counters.FirstOrDefaultAsync(c => c.Id == counterId && c.IsActive, ct);
        if (counter == null) return new StaffCounterStatusResultDto(false, "Counter not found.", status);

        var normalized = status.Trim().ToUpperInvariant();
        var mapped = normalized switch
        {
            "AVAILABLE" => CounterStatus.AVAILABLE,
            "BUSY" => CounterStatus.MAINTENANCE,
            "BREAK" => CounterStatus.OFFLINE,
            "OFFLINE" => CounterStatus.OFFLINE,
            _ => (CounterStatus?)null
        };
        if (mapped == null)
            return new StaffCounterStatusResultDto(false, "Unsupported counter status.", status);

        if (mapped == CounterStatus.AVAILABLE)
        {
            var hasActive = await _db.Tokens.AnyAsync(
                t => t.CounterId == counterId && (t.Status == TokenStatus.CALLED || t.Status == TokenStatus.SERVING), ct);
            if (hasActive)
                return new StaffCounterStatusResultDto(false, await GetDisplayMessageAsync("STAFF_COUNTER_STATUS_ACTIVE_BLOCK", "Complete current token before setting available.", ct), counter.Status.ToString());
        }

        counter.Status = mapped.Value;
        counter.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);

        var messageKey = $"STAFF_COUNTER_STATUS_{normalized}_OK";
        var message = await GetDisplayMessageAsync(messageKey, $"Counter status updated to {mapped}.", ct);
        await _notify.QueueUpdatedAsync(new { counterId, status = mapped.ToString() }, ct);
        return new StaffCounterStatusResultDto(true, message, mapped.Value.ToString());
    }

    private async Task<TokenActionResultDto?> UpdateStatusAsync(int tokenId, int counterId, int? staffUserId, TokenStatus targetStatus, TokenStatus[] allowed, string signalName, string defaultMessage, CancellationToken ct, bool setStartedAt = false, bool setCompletedAt = false, bool setSkippedAt = false, bool setCancelledAt = false, string? remarks = null, bool allowUnassignedCounter = false)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        var token = await _db.Tokens.Include(t => t.Service).Include(t => t.SubService).FirstOrDefaultAsync(t => t.Id == tokenId, ct);
        if (token == null) return null;
        if (!allowUnassignedCounter && token.CounterId != counterId && !await IsAdminAsync(staffUserId, ct))
            return new TokenActionResultDto(false, "Unauthorized for this token.", null);
        if (!allowed.Contains(token.Status)) return new TokenActionResultDto(false, $"Invalid token status transition from {token.Status}.", null);

        var oldStatus = token.Status;
        token.Status = targetStatus;
        if (setStartedAt) token.StartedAt = DateTime.Now;
        if (setCompletedAt) token.CompletedAt = DateTime.Now;
        if (setSkippedAt) token.SkippedAt = DateTime.Now;
        if (setCancelledAt) token.CancelledAt = DateTime.Now;
        if (targetStatus is TokenStatus.COMPLETED or TokenStatus.SKIPPED)
        {
            var hasOtherActive = await _db.Tokens.AnyAsync(t => t.CounterId == counterId && t.Id != token.Id && (t.Status == TokenStatus.CALLED || t.Status == TokenStatus.SERVING), ct);
            if (!hasOtherActive)
            {
                var counter = await _db.Counters.FirstOrDefaultAsync(c => c.Id == counterId, ct);
                if (counter != null) counter.Status = CounterStatus.AVAILABLE;
            }
        }

        _db.TokenStatusHistories.Add(new TokenStatusHistory
        {
            TokenId = token.Id,
            OldStatus = oldStatus,
            NewStatus = targetStatus,
            CounterId = counterId,
            StaffUserId = staffUserId,
            ChangedAt = DateTime.Now,
            Remarks = remarks ?? defaultMessage
        });
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var dto = MapActiveSession(token);
        await PublishSignal(signalName, dto, ct);
        await _notify.QueueUpdatedAsync(new { counterId, tokenId }, ct);
        await _notify.DisplayUpdatedAsync(await new DisplayService(_db).GetDisplayBoardAsync(ct), ct);
        return new TokenActionResultDto(true, defaultMessage, dto);
    }

    private async Task PublishSignal(string signalName, object payload, CancellationToken ct)
    {
        if (signalName == "TokenStarted") await _notify.TokenStartedAsync(payload, ct);
        else if (signalName == "TokenCompleted") await _notify.TokenCompletedAsync(payload, ct);
        else if (signalName == "TokenSkipped") await _notify.TokenSkippedAsync(payload, ct);
        else if (signalName == "TokenRecalled") await _notify.TokenRecalledAsync(payload, ct);
    }

    private static StaffActiveSessionDto MapActiveSession(Token token)
    {
        var start = token.StartedAt ?? token.CalledAt ?? token.CreatedAt;
        var elapsed = Math.Max(0, (int)(DateTime.Now - start).TotalSeconds);
        return new StaffActiveSessionDto(
            token.Id,
            FormatStaffTokenNo(token),
            token.Status.ToString(),
            token.Service.Name,
            token.SubService.Name,
            token.Priority.ToString(),
            token.CalledAt,
            token.StartedAt,
            elapsed,
            token.SubService.EstimatedServiceMinutes,
            true,
            true);
    }

    private static string FormatDuration(int seconds)
    {
        if (seconds <= 0) return "0m 00s";
        var mins = seconds / 60;
        var sec = seconds % 60;
        return $"{mins}m {sec:00}s";
    }

    private static string FormatSignedPercent(decimal value)
    {
        var rounded = Math.Round(value);
        return rounded >= 0 ? $"+{rounded}%" : $"{rounded}%";
    }

    private static string BuildServedTrendLabel(int current, int previous)
    {
        if (previous == 0) return current > 0 ? "+100%" : "0%";
        var pct = Math.Round((decimal)(current - previous) / previous * 100);
        return pct >= 0 ? $"+{pct}%" : $"{pct}%";
    }

    private static string BuildAvgServiceTrendLabel(int currentSeconds, int previousSeconds)
    {
        if (previousSeconds <= 0) return "Stable";
        var delta = previousSeconds - currentSeconds;
        if (delta == 0) return "Stable";
        return FormatDurationDelta(delta);
    }

    private static string FormatDurationDelta(int seconds)
    {
        var sign = seconds >= 0 ? "-" : "+";
        var abs = Math.Abs(seconds);
        var mins = abs / 60;
        var sec = abs % 60;
        return $"{sign}{mins}:{sec:00}";
    }

    private static bool TokenTouchedInPeriod(Token token, DateTime periodStart, DateTime periodEnd) =>
        (token.CreatedAt >= periodStart && token.CreatedAt < periodEnd) ||
        (token.CalledAt >= periodStart && token.CalledAt < periodEnd) ||
        (token.StartedAt >= periodStart && token.StartedAt < periodEnd) ||
        (token.CompletedAt >= periodStart && token.CompletedAt < periodEnd) ||
        (token.SkippedAt >= periodStart && token.SkippedAt < periodEnd) ||
        (token.Status is TokenStatus.CALLED or TokenStatus.SERVING &&
         token.CalledAt >= periodStart && token.CalledAt < periodEnd);

    private static DateTime? GetTokenActivityTime(Token token) =>
        token.CompletedAt ?? token.SkippedAt ?? token.StartedAt ?? token.CalledAt;

    private static (int Cash, int Account, int Loan) ClassifyHourlyTraffic(IReadOnlyList<Token> hourTokens)
    {
        var cash = 0;
        var account = 0;
        var loan = 0;

        foreach (var token in hourTokens)
        {
            var code = token.Service?.Code ?? string.Empty;
            var prefix = token.TokenPrefix ?? string.Empty;

            if (code.Equals("CASH", StringComparison.OrdinalIgnoreCase) ||
                prefix.Equals("CW", StringComparison.OrdinalIgnoreCase) ||
                prefix.Equals("CD", StringComparison.OrdinalIgnoreCase))
            {
                cash++;
                continue;
            }

            if (code.Equals("LOAN", StringComparison.OrdinalIgnoreCase) ||
                prefix.StartsWith("LN", StringComparison.OrdinalIgnoreCase))
            {
                loan++;
                continue;
            }

            account++;
        }

        return (cash, account, loan);
    }

    private static IReadOnlyList<HourlyTrafficPointDto> BuildHourlyTraffic(IReadOnlyList<Token> activityTokens)
    {
        var minHour = 8;
        var maxHour = 17;
        var timedTokens = activityTokens
            .Select(t => new { Token = t, At = GetTokenActivityTime(t) })
            .Where(x => x.At.HasValue)
            .ToList();

        if (timedTokens.Count > 0)
        {
            var hours = timedTokens.Select(x => x.At!.Value.Hour).ToList();
            minHour = Math.Min(8, hours.Min());
            maxHour = Math.Max(17, hours.Max());
        }

        var points = new List<HourlyTrafficPointDto>();
        for (var hour = minHour; hour <= maxHour; hour++)
        {
            var hourTokens = timedTokens.Where(x => x.At!.Value.Hour == hour).Select(x => x.Token).ToList();
            var (cash, account, loan) = ClassifyHourlyTraffic(hourTokens);
            points.Add(new HourlyTrafficPointDto($"{hour:00}:00", cash, account, loan));
        }

        if (points.Count == 0)
        {
            for (var hour = 8; hour <= 17; hour++)
                points.Add(new HourlyTrafficPointDto($"{hour:00}:00", 0, 0, 0));
        }

        return points;
    }

    private async Task<IReadOnlyList<StaffTimelineItemDto>> BuildPerformanceTimelineAsync(
        int counterId,
        DateTime periodStart,
        DateTime periodEnd,
        IReadOnlyList<Token> activityTokens,
        CancellationToken ct)
    {
        var items = new List<StaffTimelineItemDto>();

        foreach (var token in activityTokens)
        {
            if (token.CompletedAt >= periodStart && token.CompletedAt < periodEnd)
            {
                var serviceSeconds = token.StartedAt.HasValue
                    ? (int)(token.CompletedAt!.Value - token.StartedAt.Value).TotalSeconds
                    : 0;
                items.Add(new StaffTimelineItemDto(
                    "COMPLETED",
                    token.TokenNo,
                    $"{token.TokenNo} Completed",
                    token.SubService.Name,
                    serviceSeconds > 0 ? "Service Time" : null,
                    serviceSeconds > 0 ? FormatDuration(serviceSeconds) : null,
                    token.CompletedAt.Value));
                continue;
            }

            if (token.SkippedAt >= periodStart && token.SkippedAt < periodEnd)
            {
                var waitSeconds = token.CalledAt.HasValue
                    ? (int)(token.SkippedAt!.Value - token.CalledAt.Value).TotalSeconds
                    : token.QueuedAt.HasValue
                        ? (int)(token.SkippedAt.Value - token.QueuedAt.Value).TotalSeconds
                        : 0;
                items.Add(new StaffTimelineItemDto(
                    "SKIPPED",
                    token.TokenNo,
                    $"{token.TokenNo} Skipped",
                    $"{token.SubService.Name} • No show",
                    waitSeconds > 0 ? "Wait time" : null,
                    waitSeconds > 0 ? FormatDuration(waitSeconds) : null,
                    token.SkippedAt.Value));
                continue;
            }

            if (token.Status == TokenStatus.SERVING && token.StartedAt.HasValue)
            {
                var elapsed = (int)(DateTime.Now - token.StartedAt.Value).TotalSeconds;
                items.Add(new StaffTimelineItemDto(
                    "SERVING",
                    token.TokenNo,
                    $"{token.TokenNo} In Service",
                    token.SubService.Name,
                    "Elapsed",
                    FormatDuration(elapsed),
                    token.StartedAt.Value));
                continue;
            }

            if (token.CalledAt >= periodStart && token.CalledAt < periodEnd &&
                token.Status is TokenStatus.CALLED or TokenStatus.SERVING)
            {
                var waitMinutes = token.QueuedAt.HasValue
                    ? Math.Max(0, (int)(token.CalledAt.Value - token.QueuedAt.Value).TotalMinutes)
                    : Math.Max(0, (int)(token.CalledAt.Value - token.CreatedAt).TotalMinutes);
                items.Add(new StaffTimelineItemDto(
                    "CALLED",
                    token.TokenNo,
                    $"{token.TokenNo} Called",
                    token.SubService.Name,
                    "Wait time",
                    FormatWaitDuration(waitMinutes),
                    token.CalledAt.Value));
            }
        }

        if (items.Count == 0)
        {
            var histories = await _db.TokenStatusHistories.AsNoTracking()
                .Include(h => h.Token).ThenInclude(t => t.SubService)
                .Where(h => h.CounterId == counterId && h.ChangedAt >= periodStart && h.ChangedAt < periodEnd)
                .OrderByDescending(h => h.ChangedAt)
                .Take(20)
                .ToListAsync(ct);

            foreach (var history in histories)
            {
                items.Add(new StaffTimelineItemDto(
                    history.NewStatus.ToString(),
                    history.Token.TokenNo,
                    $"{history.Token.TokenNo} {history.NewStatus}",
                    history.Remarks ?? history.Token.SubService.Name,
                    null,
                    null,
                    history.ChangedAt));
            }
        }

        if (items.Count == 0)
        {
            for (var day = periodStart.Date; day < periodEnd.Date; day = day.AddDays(1))
            {
                var historyItems = await GetTokenHistoryAsync(counterId, day, null, null, null, null, ct);
                foreach (var row in historyItems.Take(12))
                {
                    var eventType = row.Status.ToUpperInvariant();
                    var timestamp = row.CalledTime ?? day.AddHours(12);
                    items.Add(new StaffTimelineItemDto(
                        eventType,
                        row.TokenNo,
                        $"{row.TokenNo} {row.Status}",
                        row.ServiceType,
                        row.Duration != "--" ? "Duration" : null,
                        row.Duration != "--" ? row.Duration : null,
                        timestamp));
                }
            }
        }

        return items
            .OrderByDescending(i => i.Timestamp)
            .Take(12)
            .ToList();
    }

    private async Task<string> BuildPerformanceTipAsync(
        IReadOnlyList<Token> completed,
        IReadOnlyList<HourlyTrafficPointDto> hourlyTraffic,
        CancellationToken ct)
    {
        if (completed.Count == 0)
            return await GetDisplayMessageAsync("STAFF_PERFORMANCE_TIP", "Maintain current service pace during peak slots.", ct);

        var bestSubService = completed
            .Where(t => t.StartedAt.HasValue && t.CompletedAt.HasValue)
            .GroupBy(t => t.SubService.Name)
            .Select(g => new
            {
                Name = g.Key,
                AvgSeconds = g.Average(t => (t.CompletedAt!.Value - t.StartedAt!.Value).TotalSeconds)
            })
            .OrderBy(x => x.AvgSeconds)
            .FirstOrDefault();

        var peakHour = hourlyTraffic
            .OrderByDescending(h => h.CashCount + h.AccountCount + h.LoanCount)
            .FirstOrDefault();

        if (bestSubService == null || peakHour == null)
            return await GetDisplayMessageAsync("STAFF_PERFORMANCE_TIP", "Maintain current service pace during peak slots.", ct);

        return $"Your service time for '{bestSubService.Name}' is exceptionally low. Try maintaining this momentum during the {peakHour.HourLabel} peak to reach the Branch Champion status.";
    }

    private async Task<List<int>> GetAssignedServiceIdsAsync(int counterId, CancellationToken ct) =>
        await _db.CounterServiceAssignments.AsNoTracking()
            .Where(a => a.CounterId == counterId && a.IsActive)
            .Select(a => a.ServiceId)
            .Distinct()
            .ToListAsync(ct);

    private Task<(int SequenceNo, string TokenNo)> GetNextTokenForSubServiceAsync(
        int subServiceId, string tokenPrefix, CancellationToken ct) =>
        TokenSequenceHelper.NextAsync(_db, subServiceId, tokenPrefix, ct);

    private async Task<string> GetSettingAsync(string key, string fallback, CancellationToken ct)
    {
        var value = await _db.SystemSettings.AsNoTracking().Where(s => s.SettingKey == key && s.IsActive).Select(s => s.SettingValue).FirstOrDefaultAsync(ct);
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private async Task<int> GetSettingIntAsync(string key, int fallback, CancellationToken ct)
    {
        var val = await GetSettingAsync(key, fallback.ToString(), ct);
        return int.TryParse(val, out var parsed) ? parsed : fallback;
    }

    private async Task<bool> GetSettingBoolAsync(string key, bool fallback, CancellationToken ct)
    {
        var val = await GetSettingAsync(key, fallback.ToString(), ct);
        return bool.TryParse(val, out var parsed) ? parsed : fallback;
    }

    private async Task<string> GetDisplayMessageAsync(string key, string fallback, CancellationToken ct)
    {
        var message = await _db.DisplayMessages.AsNoTracking().Where(x => x.MessageKey == key && x.IsActive).Select(x => x.MessageText).FirstOrDefaultAsync(ct);
        return string.IsNullOrWhiteSpace(message) ? fallback : message;
    }

    private async Task<bool> IsAdminAsync(int? staffUserId, CancellationToken ct)
    {
        if (!staffUserId.HasValue) return false;
        return await _db.StaffUsers.AsNoTracking().AnyAsync(s => s.Id == staffUserId.Value && s.Role == StaffRole.ADMIN, ct);
    }

    private static string FormatStaffTokenNo(Token token)
    {
        if (token.TransferCount <= 0)
        {
            return token.TokenNo;
        }

        if (!string.IsNullOrWhiteSpace(token.TransferredFromTokenNo))
        {
            return $"{token.TransferredFromTokenNo} ({token.TokenNo}(T))";
        }

        return $"{token.TokenNo} (T)";
    }

    private async Task<string> ResolveCustomerLabelAsync(TokenPriority priority, CancellationToken ct)
    {
        var isPriority = priority != TokenPriority.STANDARD;
        var key = isPriority ? "STAFF_CUSTOMER_LABEL_VIP" : "STAFF_CUSTOMER_LABEL_STANDARD";
        var fallback = isPriority ? "Priority Member" : "Regular Member";
        return await GetDisplayMessageAsync(key, fallback, ct);
    }

    private async Task<StaffMyCounterEfficiencyDto> BuildMyCounterEfficiencyAsync(StaffPerformanceDto performance, CancellationToken ct)
    {
        var breakAllowance = await GetSettingIntAsync("STAFF_BREAK_ALLOWANCE_MINUTES", 60, ct);
        var breakUsed = await GetSettingIntAsync("STAFF_BREAK_USED_MINUTES", 0, ct);
        var shiftEndRaw = await GetSettingAsync("STAFF_SHIFT_END_TIME", "17:00", ct);
        var trend = await GetDisplayMessageAsync("STAFF_EFFICIENCY_TREND", "+2% since last hour", ct);

        var shiftEndsIn = "--";
        if (TimeSpan.TryParse(shiftEndRaw, out var shiftEnd))
        {
            var endToday = DateTime.Today.Add(shiftEnd);
            if (endToday > DateTime.Now)
                shiftEndsIn = FormatDuration((int)(endToday - DateTime.Now).TotalSeconds);
            else
                shiftEndsIn = await GetDisplayMessageAsync("STAFF_SHIFT_ENDED", "Shift ended", ct);
        }

        return new StaffMyCounterEfficiencyDto(
            (int)Math.Round(performance.CompletionRate),
            trend,
            $"{breakUsed}m / {breakAllowance}m",
            $"{performance.CompletionRate:0}%",
            shiftEndsIn);
    }

    private static string ExtractTokenPrefixBadge(string tokenNo)
    {
        var idx = tokenNo.IndexOf('-');
        if (idx <= 0) return tokenNo.Length >= 2 ? tokenNo[..2].ToUpperInvariant() : tokenNo;
        return tokenNo[..idx].ToUpperInvariant();
    }

    private static string FormatWaitDuration(int minutes)
    {
        if (minutes < 60) return $"{minutes}m";
        var h = minutes / 60;
        var m = minutes % 60;
        return m == 0 ? $"{h}h" : $"{h}h {m}m";
    }

    private static (string Title, string Subtitle) MapJourneyStep(
        TokenStatus newStatus,
        TokenStatus? oldStatus,
        DateTime changedAt,
        string? remarks,
        string? counterName,
        string kioskLabel)
    {
        var time = changedAt.ToString("HH:mm:ss");
        return newStatus switch
        {
            TokenStatus.WAITING when oldStatus == null => ("Token Generated", $"{kioskLabel} • {time}"),
            TokenStatus.WAITING => ("Enters Waiting Pool", $"Global Queue • {time}"),
            TokenStatus.CALLED => ("Token Called", $"{counterName ?? "Counter"} • {time}"),
            TokenStatus.SERVING => ("Service Started", $"{counterName ?? "Counter"} • {time}"),
            TokenStatus.COMPLETED => ("Service Completed", $"{counterName ?? "Counter"} • {time}"),
            TokenStatus.SKIPPED => ("Marked No Show", string.IsNullOrWhiteSpace(remarks) ? time : $"{remarks} • {time}"),
            TokenStatus.CANCELLED => ("Token Cancelled", string.IsNullOrWhiteSpace(remarks) ? time : $"{remarks} • {time}"),
            TokenStatus.TRANSFERRED => ("Token Transferred", string.IsNullOrWhiteSpace(remarks) ? time : $"{remarks} • {time}"),
            _ => (newStatus.ToString(), time)
        };
    }
}
