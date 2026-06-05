using SmartQ.Application.DTOs;

namespace SmartQ.Application.Interfaces;

public interface IStaffSessionService
{
    Task<IReadOnlyList<AvailableCounterDto>> GetAvailableCountersAsync(CancellationToken ct = default);
    Task<StaffCounterSessionResultDto> SelectCounterAsync(int staffUserId, SelectCounterRequest request, string? loginIp, CancellationToken ct = default);
    Task EndSessionAsync(int staffUserId, CancellationToken ct = default);
    Task<StaffCounterSessionResultDto> UpdateSessionStatusAsync(int staffUserId, string status, CancellationToken ct = default);
    Task<ActiveCounterSessionDto?> GetActiveSessionForStaffAsync(int staffUserId, CancellationToken ct = default);
    Task<int?> ResolveCounterIdForStaffAsync(int staffUserId, CancellationToken ct = default);
    Task ForceCloseSessionAsync(int staffUserId, CancellationToken ct = default);
}
