using Microsoft.EntityFrameworkCore;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;
using SmartQ.Domain.Entities;
using SmartQ.Domain.Enums;
using SmartQ.Infrastructure.Persistence;

namespace SmartQ.Infrastructure.Services;

public class StaffSessionService : IStaffSessionService
{
    private static readonly StaffCounterSessionStatus[] ActiveStatuses =
        [StaffCounterSessionStatus.ACTIVE, StaffCounterSessionStatus.BREAK];

    private readonly SmartQDbContext _db;

    public StaffSessionService(SmartQDbContext db) => _db = db;

    public async Task<IReadOnlyList<AvailableCounterDto>> GetAvailableCountersAsync(CancellationToken ct = default)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var counters = await _db.Counters.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CounterNo)
            .Select(c => new
            {
                c.Id,
                c.CounterNo,
                c.CounterName,
                c.Status,
                Assignments = c.ServiceAssignments
                    .Where(a => a.IsActive)
                    .Select(a => new
                    {
                        a.ServiceId,
                        a.Service.Name,
                        a.Service.Code,
                        Prefixes = a.Service.SubServices.Where(ss => ss.IsActive).Select(ss => ss.TokenPrefix).ToList()
                    })
                    .ToList()
            })
            .ToListAsync(ct);

        var activeSessions = await _db.StaffCounterSessions.AsNoTracking()
            .Where(s => ActiveStatuses.Contains(s.Status))
            .Include(s => s.StaffUser)
            .ToListAsync(ct);

        return counters.Select(c =>
        {
            var occupied = activeSessions.FirstOrDefault(s => s.CounterId == c.Id);
            var offline = c.Status is CounterStatus.OFFLINE or CounterStatus.MAINTENANCE;
            var busy = occupied != null;
            var available = !offline && !busy;

            return new AvailableCounterDto(
                c.Id,
                c.CounterNo,
                c.CounterName,
                c.Status.ToString(),
                available,
                c.Assignments.Select(a => new AvailableCounterAssignedServiceDto(
                    a.ServiceId, a.Name, a.Code, a.Prefixes)).ToList(),
                occupied?.StaffUser.FullName);
        }).ToList();
    }

    public async Task<StaffCounterSessionResultDto> SelectCounterAsync(
        int staffUserId, SelectCounterRequest request, string? loginIp, CancellationToken ct = default)
    {
        var user = await _db.StaffUsers.FirstOrDefaultAsync(u => u.Id == staffUserId, ct)
            ?? throw new InvalidOperationException("Staff user not found.");

        if (user.Role != StaffRole.STAFF)
            throw new InvalidOperationException("Only staff users can select a counter.");
        if (!user.IsActive)
            throw new InvalidOperationException("Account is inactive.");

        var counter = await _db.Counters
            .Include(c => c.ServiceAssignments).ThenInclude(a => a.Service).ThenInclude(s => s.SubServices)
            .FirstOrDefaultAsync(c => c.Id == request.CounterId, ct)
            ?? throw new InvalidOperationException("Counter not found.");

        if (!counter.IsActive)
            throw new InvalidOperationException("Counter is not active.");
        if (counter.Status is CounterStatus.OFFLINE or CounterStatus.MAINTENANCE)
            throw new InvalidOperationException($"Counter is {counter.Status}.");

        var counterBusy = await _db.StaffCounterSessions.AnyAsync(
            s => s.CounterId == request.CounterId && ActiveStatuses.Contains(s.Status) && s.StaffUserId != staffUserId, ct);
        if (counterBusy)
            throw new InvalidOperationException("Counter is already in use by another staff member.");

        var existing = await _db.StaffCounterSessions
            .Where(s => s.StaffUserId == staffUserId && ActiveStatuses.Contains(s.Status))
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(ct);

        if (existing != null)
        {
            if (existing.CounterId == request.CounterId)
                return await MapSessionResultAsync(existing, ct);

            existing.Status = StaffCounterSessionStatus.ENDED;
            existing.EndedAt = DateTime.UtcNow;
        }

        var session = new StaffCounterSession
        {
            StaffUserId = staffUserId,
            CounterId = request.CounterId,
            StartedAt = DateTime.UtcNow,
            Status = StaffCounterSessionStatus.ACTIVE,
            LoginIp = loginIp,
            DeviceName = request.DeviceName?.Trim()
        };

        _db.StaffCounterSessions.Add(session);
        if (counter.Status == CounterStatus.AVAILABLE)
            counter.Status = CounterStatus.SERVING;

        await _db.SaveChangesAsync(ct);
        return await MapSessionResultAsync(session, ct);
    }

    public async Task EndSessionAsync(int staffUserId, CancellationToken ct = default)
    {
        var session = await _db.StaffCounterSessions
            .Where(s => s.StaffUserId == staffUserId && ActiveStatuses.Contains(s.Status))
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(ct);

        if (session == null) return;

        session.Status = StaffCounterSessionStatus.ENDED;
        session.EndedAt = DateTime.UtcNow;
        await ReleaseCounterIfIdleAsync(session.CounterId, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<StaffCounterSessionResultDto> UpdateSessionStatusAsync(
        int staffUserId, string status, CancellationToken ct = default)
    {
        if (!Enum.TryParse<StaffCounterSessionStatus>(status, true, out var newStatus))
            throw new InvalidOperationException("Invalid session status.");

        if (newStatus is not (StaffCounterSessionStatus.ACTIVE or StaffCounterSessionStatus.BREAK or StaffCounterSessionStatus.ENDED))
            throw new InvalidOperationException("Status not allowed.");

        var session = await _db.StaffCounterSessions
            .Where(s => s.StaffUserId == staffUserId && ActiveStatuses.Contains(s.Status))
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("No active counter session found.");

        if (newStatus == StaffCounterSessionStatus.ENDED)
        {
            session.Status = StaffCounterSessionStatus.ENDED;
            session.EndedAt = DateTime.UtcNow;
            await ReleaseCounterIfIdleAsync(session.CounterId, ct);
        }
        else
        {
            session.Status = newStatus;
        }

        await _db.SaveChangesAsync(ct);
        return await MapSessionResultAsync(session, ct);
    }

    public async Task<ActiveCounterSessionDto?> GetActiveSessionForStaffAsync(int staffUserId, CancellationToken ct = default)
    {
        var session = await _db.StaffCounterSessions.AsNoTracking()
            .Where(s => s.StaffUserId == staffUserId && ActiveStatuses.Contains(s.Status))
            .OrderByDescending(s => s.StartedAt)
            .Select(s => new ActiveCounterSessionDto(
                s.Id,
                s.CounterId,
                s.Counter.CounterNo,
                s.Counter.CounterName,
                s.Status.ToString(),
                s.StartedAt))
            .FirstOrDefaultAsync(ct);

        return session;
    }

    public async Task<int?> ResolveCounterIdForStaffAsync(int staffUserId, CancellationToken ct = default)
    {
        var session = await _db.StaffCounterSessions.AsNoTracking()
            .Where(s => s.StaffUserId == staffUserId && ActiveStatuses.Contains(s.Status))
            .OrderByDescending(s => s.StartedAt)
            .Select(s => (int?)s.CounterId)
            .FirstOrDefaultAsync(ct);

        return session;
    }

    public async Task ForceCloseSessionAsync(int staffUserId, CancellationToken ct = default)
    {
        var session = await _db.StaffCounterSessions
            .Where(s => s.StaffUserId == staffUserId && ActiveStatuses.Contains(s.Status))
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(ct);

        if (session == null) return;

        session.Status = StaffCounterSessionStatus.FORCE_CLOSED;
        session.EndedAt = DateTime.UtcNow;
        await ReleaseCounterIfIdleAsync(session.CounterId, ct);
        await _db.SaveChangesAsync(ct);
    }

    private async Task ReleaseCounterIfIdleAsync(int counterId, CancellationToken ct)
    {
        var hasActiveToken = await _db.Tokens.AnyAsync(
            t => t.CounterId == counterId && (t.Status == TokenStatus.CALLED || t.Status == TokenStatus.SERVING), ct);
        if (hasActiveToken) return;

        var counter = await _db.Counters.FirstOrDefaultAsync(c => c.Id == counterId, ct);
        if (counter != null && counter.Status == CounterStatus.SERVING)
            counter.Status = CounterStatus.AVAILABLE;
    }

    private async Task<StaffCounterSessionResultDto> MapSessionResultAsync(StaffCounterSession session, CancellationToken ct)
    {
        var counter = await _db.Counters.AsNoTracking()
            .Where(c => c.Id == session.CounterId)
            .Select(c => new
            {
                c.CounterNo,
                c.CounterName,
                Assignments = c.ServiceAssignments.Where(a => a.IsActive).Select(a => new
                {
                    a.ServiceId,
                    a.Service.Name,
                    a.Service.Code,
                    Prefixes = a.Service.SubServices.Where(ss => ss.IsActive).Select(ss => ss.TokenPrefix).ToList()
                }).ToList()
            })
            .FirstAsync(ct);

        return new StaffCounterSessionResultDto(
            session.Id,
            session.CounterId,
            counter.CounterNo,
            counter.CounterName,
            session.Status.ToString(),
            session.StartedAt,
            counter.Assignments.Select(a => new AvailableCounterAssignedServiceDto(
                a.ServiceId, a.Name, a.Code, a.Prefixes)).ToList());
    }
}
