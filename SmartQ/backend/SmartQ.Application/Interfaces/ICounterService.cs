using SmartQ.Application.DTOs;

namespace SmartQ.Application.Interfaces;

public interface ICounterService
{
    Task<IReadOnlyList<CounterDto>> GetCountersAsync(CancellationToken ct = default);
    Task<CounterQueueDto> GetCounterQueueAsync(int counterId, CancellationToken ct = default);
    Task<CallNextResponse?> CallNextAsync(int counterId, int? staffUserId = null, CancellationToken ct = default);
    Task<StaffConsoleSummaryDto> GetStaffConsoleSummaryAsync(int counterId, CancellationToken ct = default);
}
