using Microsoft.EntityFrameworkCore;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;
using SmartQ.Domain.Entities;
using SmartQ.Domain.Enums;
using SmartQ.Infrastructure.Persistence;

namespace SmartQ.Infrastructure.Services;

public class CounterService : ICounterService
{
    private readonly SmartQDbContext _db;
    private readonly IQueueNotificationService _notify;

    public CounterService(SmartQDbContext db, IQueueNotificationService notify)
    {
        _db = db;
        _notify = notify;
    }

    public async Task<IReadOnlyList<CounterDto>> GetCountersAsync(CancellationToken ct = default)
    {
        return await _db.Counters.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CounterNo)
            .Select(c => new CounterDto(c.Id, c.CounterNo, c.CounterName, c.Status.ToString(), c.IsActive))
            .ToListAsync(ct);
    }

    private async Task<List<int>> GetAssignedServiceIdsAsync(int counterId, CancellationToken ct) =>
        await _db.CounterServiceAssignments.AsNoTracking()
            .Where(a => a.CounterId == counterId && a.IsActive)
            .Select(a => a.ServiceId)
            .ToListAsync(ct);

    private async Task<List<string>> GetAssignedServiceNamesAsync(int counterId, CancellationToken ct) =>
        await _db.CounterServiceAssignments.AsNoTracking()
            .Where(a => a.CounterId == counterId && a.IsActive)
            .OrderBy(a => a.Service.DisplayOrder)
            .Select(a => a.Service.Name)
            .ToListAsync(ct);

    public async Task<CounterQueueDto> GetCounterQueueAsync(int counterId, CancellationToken ct = default)
    {
        var counter = await _db.Counters.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == counterId, ct)
            ?? throw new InvalidOperationException("Counter not found.");

        var serviceIds = await GetAssignedServiceIdsAsync(counterId, ct);
        var assignedNames = await GetAssignedServiceNamesAsync(counterId, ct);

        var priorityEnabled = await GetSettingBoolAsync("ENABLE_PRIORITY_QUEUE", true, ct);

        var waitingQuery = _db.Tokens.AsNoTracking()
            .Include(t => t.Service)
            .Include(t => t.SubService)
            .Where(t => t.Status == TokenStatus.WAITING && serviceIds.Contains(t.ServiceId));

        if (priorityEnabled)
            waitingQuery = waitingQuery.OrderByDescending(t => t.Priority != TokenPriority.STANDARD)
                .ThenBy(t => t.QueuedAt ?? t.CreatedAt)
                .ThenBy(t => t.CreatedAt);
        else
            waitingQuery = waitingQuery.OrderBy(t => t.QueuedAt ?? t.CreatedAt)
                .ThenBy(t => t.CreatedAt);

        var waiting = await waitingQuery.Take(50).ToListAsync(ct);

        var active = await _db.Tokens.AsNoTracking()
            .Include(t => t.Service).Include(t => t.SubService).Include(t => t.Counter)
            .Where(t => t.CounterId == counterId && (t.Status == TokenStatus.CALLED || t.Status == TokenStatus.SERVING))
            .OrderByDescending(t => t.CalledAt)
            .FirstOrDefaultAsync(ct);

        var next = waiting.FirstOrDefault();

        return new CounterQueueDto(
            counter.Id, counter.CounterName, counter.Status.ToString(),
            assignedNames,
            active != null ? MapDetail(active) : null,
            next != null ? MapDetail(next) : null,
            waiting.Select(MapQueue).ToList());
    }

    public async Task<CallNextResponse?> CallNextAsync(int counterId, int? staffUserId = null, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var counter = await _db.Counters
                .FirstOrDefaultAsync(c => c.Id == counterId && c.IsActive, ct)
                ?? throw new InvalidOperationException("Counter not found.");

            var serviceIds = await GetAssignedServiceIdsAsync(counterId, ct);

            if (serviceIds.Count == 0)
                throw new InvalidOperationException("No services assigned to counter.");

            var priorityEnabled = await GetSettingBoolAsync("ENABLE_PRIORITY_QUEUE", true, ct);

            IQueryable<Token> query = _db.Tokens
                .Include(t => t.Service)
                .Include(t => t.SubService)
                .Where(t => t.Status == TokenStatus.WAITING && serviceIds.Contains(t.ServiceId));

            if (priorityEnabled)
                query = query.OrderByDescending(t => t.Priority != TokenPriority.STANDARD)
                    .ThenBy(t => t.QueuedAt ?? t.CreatedAt)
                    .ThenBy(t => t.CreatedAt);
            else
                query = query.OrderBy(t => t.QueuedAt ?? t.CreatedAt)
                    .ThenBy(t => t.CreatedAt);

            var token = await query.FirstOrDefaultAsync(ct);
            if (token == null) return null;

            var oldStatus = token.Status;
            token.Status = TokenStatus.CALLED;
            token.CounterId = counterId;
            token.CalledAt = DateTime.Now;
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

            var response = new CallNextResponse(
                token.Id, token.TokenNo, token.Service.Name, token.SubService.Name,
                counter.CounterName, counter.CounterNo);

            await _notify.TokenCalledAsync(response, ct);
            await _notify.QueueUpdatedAsync(new { counterId }, ct);

            var display = new DisplayService(_db);
            await _notify.DisplayUpdatedAsync(await display.GetDisplayBoardAsync(ct), ct);

            return response;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<StaffConsoleSummaryDto> GetStaffConsoleSummaryAsync(int counterId, CancellationToken ct = default)
    {
        var today = DateTime.Today;
        var serviceIds = await GetAssignedServiceIdsAsync(counterId, ct);

        var waitingQuery = _db.Tokens.AsNoTracking()
            .Where(t => t.Status == TokenStatus.WAITING);
        if (serviceIds.Count > 0)
            waitingQuery = waitingQuery.Where(t => serviceIds.Contains(t.ServiceId));

        var waiting = await waitingQuery.CountAsync(ct);

        var served = await _db.Tokens.AsNoTracking()
            .CountAsync(t => t.CounterId == counterId && t.CompletedAt >= today, ct);

        var avgWait = await waitingQuery
            .Select(t => (int?)t.EstimatedWaitMinutes)
            .AverageAsync(ct) ?? 0;

        var queue = await GetCounterQueueAsync(counterId, ct);
        return new StaffConsoleSummaryDto(waiting, served, (int)avgWait, queue);
    }

    private static TokenDetailDto MapDetail(Token t) => new(
        t.Id, t.TokenNo, t.Service.Name, t.SubService.Name,
        t.Status.ToString(), t.Priority.ToString(), t.CounterId,
        t.Counter?.CounterName, t.EstimatedWaitMinutes, t.CreatedAt, t.CalledAt);

    private static QueueTokenDto MapQueue(Token t) => new(
        t.Id, t.TokenNo, t.Service.Name, t.SubService.Name,
        t.Status.ToString(), t.Priority.ToString(),
        (int)(DateTime.Now - t.CreatedAt).TotalMinutes, t.CreatedAt);

    private async Task<bool> GetSettingBoolAsync(string key, bool fallback, CancellationToken ct)
    {
        var s = await _db.SystemSettings.AsNoTracking().FirstOrDefaultAsync(x => x.SettingKey == key, ct);
        return s != null && bool.TryParse(s.SettingValue, out var v) ? v : fallback;
    }
}
