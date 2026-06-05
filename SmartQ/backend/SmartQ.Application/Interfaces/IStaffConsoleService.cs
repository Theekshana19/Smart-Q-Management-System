using SmartQ.Application.DTOs;

namespace SmartQ.Application.Interfaces;

public interface IStaffConsoleService
{
    Task<StaffConsoleContextDto> GetContextAsync(int counterId, CancellationToken ct = default);
    Task<StaffConsoleSummaryV2Dto> GetSummaryAsync(int counterId, CancellationToken ct = default);
    Task<StaffActiveSessionDto?> GetActiveSessionAsync(int counterId, CancellationToken ct = default);
    Task<IReadOnlyList<StaffQueueItemDto>> GetQueueAsync(int counterId, string scope, CancellationToken ct = default);
    Task<CallNextActionResultDto> CallNextAsync(int counterId, int? staffUserId = null, CancellationToken ct = default);
    Task<TokenActionResultDto?> RecallAsync(int tokenId, int counterId, int? staffUserId = null, CancellationToken ct = default);
    Task<TokenActionResultDto?> StartServiceAsync(int tokenId, int counterId, int? staffUserId = null, CancellationToken ct = default);
    Task<TokenActionResultDto?> CompleteAsync(int tokenId, int counterId, int? staffUserId = null, CancellationToken ct = default);
    Task<TokenActionResultDto?> NoShowAsync(int tokenId, int counterId, int? staffUserId = null, CancellationToken ct = default);
    Task<TokenActionResultDto?> CancelAsync(int tokenId, int counterId, int? staffUserId = null, CancellationToken ct = default);
    Task<TokenActionResultDto?> TransferAsync(int tokenId, StaffTransferTokenRequest request, int counterId, int? staffUserId = null, CancellationToken ct = default);
    Task<IReadOnlyList<StaffTokenHistoryItemDto>> GetTokenHistoryAsync(int counterId, DateTime? date, DateTime? dateFrom, DateTime? dateTo, string? status, int? serviceId, CancellationToken ct = default);
    Task<StaffPerformanceDto> GetPerformanceAsync(int? staffUserId, int counterId, string range, CancellationToken ct = default);
    Task<StaffNotificationResponseDto> GetNotificationsAsync(int counterId, CancellationToken ct = default);
    Task<StaffDashboardDto> GetDashboardAsync(int counterId, CancellationToken ct = default);
    Task<StaffTokenDetailsDto?> GetTokenDetailsAsync(int tokenId, CancellationToken ct = default);
    Task<StaffTransferOptionsDto> GetTransferOptionsAsync(CancellationToken ct = default);
    Task<StaffMyCounterDto> GetMyCounterAsync(int counterId, int? staffUserId = null, CancellationToken ct = default);
    Task<StaffCounterStatusResultDto> UpdateCounterStatusAsync(int counterId, string status, CancellationToken ct = default);
}
