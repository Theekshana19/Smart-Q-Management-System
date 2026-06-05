using SmartQ.Application.DTOs;

namespace SmartQ.Application.Interfaces;

public interface IAdminService
{
    Task<AdminProfileDto> GetProfileAsync(CancellationToken ct = default);
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default);

    Task<ServiceManagementSummaryDto> GetServiceManagementSummaryAsync(CancellationToken ct = default);
    Task<PagedResult<AdminServiceListItemDto>> GetServicesAsync(ServiceListQuery query, CancellationToken ct = default);
    Task<AdminServiceListItemDto> CreateServiceAsync(UpsertServiceRequest request, CancellationToken ct = default);
    Task<AdminServiceListItemDto?> UpdateServiceAsync(int id, UpsertServiceRequest request, CancellationToken ct = default);
    Task<AdminServiceListItemDto?> PatchServiceStatusAsync(int id, PatchStatusRequest request, CancellationToken ct = default);
    Task<bool> DeleteServiceAsync(int id, CancellationToken ct = default);

    Task<PagedResult<AdminSubServiceListItemDto>> GetSubServicesAsync(SubServiceListQuery query, CancellationToken ct = default);
    Task<AdminSubServiceListItemDto> CreateSubServiceAsync(UpsertSubServiceRequest request, CancellationToken ct = default);
    Task<AdminSubServiceListItemDto?> UpdateSubServiceAsync(int id, UpsertSubServiceRequest request, CancellationToken ct = default);
    Task<AdminSubServiceListItemDto?> PatchSubServiceStatusAsync(int id, PatchStatusRequest request, CancellationToken ct = default);
    Task<bool> DeleteSubServiceAsync(int id, CancellationToken ct = default);

    Task<PagedResult<AdminCounterListItemDto>> GetCountersAsync(CancellationToken ct = default);
    Task<CounterManagementDto> GetCounterManagementAsync(CancellationToken ct = default);
    Task<CounterResourceLoadDto> GetCounterResourceLoadAsync(CancellationToken ct = default);
    Task<AdminCounterListItemDto> CreateCounterAsync(UpsertCounterRequest request, CancellationToken ct = default);
    Task<AdminCounterListItemDto?> UpdateCounterAsync(int id, UpsertCounterRequest request, CancellationToken ct = default);
    Task<AdminCounterListItemDto?> PatchCounterStatusAsync(int id, PatchCounterStatusRequest request, CancellationToken ct = default);
    Task<bool> DeleteCounterAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<CounterAssignmentListItemDto>> GetCounterAssignmentsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AssignableServiceDto>> GetAssignableServicesAsync(int counterId, CancellationToken ct = default);
    Task SaveCounterAssignmentsAsync(SaveCounterAssignmentRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<SystemSettingDto>> GetSettingsAsync(CancellationToken ct = default);
    Task<SystemSettingDto?> UpdateSettingAsync(int id, UpdateSettingRequest request, CancellationToken ct = default);
    Task<TokenHistoryReportDto> GetTokenHistoryReportAsync(TokenHistoryFilter filter, CancellationToken ct = default);

    Task<StaffManagementSummaryDto> GetStaffManagementSummaryAsync(CancellationToken ct = default);
    Task<PagedResult<AdminStaffListItemDto>> GetStaffAsync(StaffListQuery query, CancellationToken ct = default);
    Task<AdminStaffListItemDto> CreateStaffAsync(CreateStaffRequest request, CancellationToken ct = default);
    Task<AdminStaffListItemDto?> UpdateStaffAsync(int id, UpdateStaffRequest request, CancellationToken ct = default);
    Task<bool> ResetStaffPasswordAsync(int id, ResetPasswordRequest request, CancellationToken ct = default);
    Task<AdminStaffListItemDto?> PatchStaffStatusAsync(int id, PatchStaffStatusRequest request, CancellationToken ct = default);
    Task<bool> ForceLogoutStaffAsync(int id, CancellationToken ct = default);
}
