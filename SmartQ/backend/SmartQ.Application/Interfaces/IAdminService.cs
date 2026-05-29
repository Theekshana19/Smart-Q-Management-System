using SmartQ.Application.DTOs;

namespace SmartQ.Application.Interfaces;

public interface IAdminService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AdminServiceDto>> GetServicesAsync(CancellationToken ct = default);
    Task<AdminServiceDto> CreateServiceAsync(UpsertServiceRequest request, CancellationToken ct = default);
    Task<AdminServiceDto?> UpdateServiceAsync(int id, UpsertServiceRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<AdminSubServiceDto>> GetSubServicesAsync(CancellationToken ct = default);
    Task<AdminSubServiceDto> CreateSubServiceAsync(UpsertSubServiceRequest request, CancellationToken ct = default);
    Task<AdminSubServiceDto?> UpdateSubServiceAsync(int id, UpsertSubServiceRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<AdminCounterDto>> GetCountersAsync(CancellationToken ct = default);
    Task<AdminCounterDto> CreateCounterAsync(UpsertCounterRequest request, CancellationToken ct = default);
    Task<AdminCounterDto?> UpdateCounterAsync(int id, UpsertCounterRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<SystemSettingDto>> GetSettingsAsync(CancellationToken ct = default);
    Task<SystemSettingDto?> UpdateSettingAsync(int id, UpdateSettingRequest request, CancellationToken ct = default);
    Task<TokenHistoryReportDto> GetTokenHistoryReportAsync(TokenHistoryFilter filter, CancellationToken ct = default);
    Task<ServiceManagementSummaryDto> GetServiceManagementSummaryAsync(CancellationToken ct = default);
}
