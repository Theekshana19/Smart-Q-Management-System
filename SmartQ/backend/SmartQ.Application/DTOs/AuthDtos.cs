namespace SmartQ.Application.DTOs;

public record LoginRequest(string Username, string Password);

public record AuthUserDto(int Id, string FullName, string Username, string Role);

public record ActiveCounterSessionDto(
    int SessionId,
    int CounterId,
    string CounterNo,
    string CounterName,
    string Status,
    DateTime StartedAt);

public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAt,
    AuthUserDto User,
    bool RequiresCounterSelection);

public record MeResponse(
    int Id,
    string FullName,
    string Username,
    string Role,
    ActiveCounterSessionDto? ActiveCounterSession);

public record SelectCounterRequest(int CounterId, string? DeviceName);

public record UpdateSessionStatusRequest(string Status);

public record AvailableCounterAssignedServiceDto(
    int ServiceId,
    string ServiceName,
    string ServiceCode,
    IReadOnlyList<string> TokenPrefixes);

public record AvailableCounterDto(
    int CounterId,
    string CounterNo,
    string CounterName,
    string Status,
    bool IsAvailableForLogin,
    IReadOnlyList<AvailableCounterAssignedServiceDto> AssignedServices,
    string? ActiveStaffName);

public record StaffCounterSessionResultDto(
    int SessionId,
    int CounterId,
    string CounterNo,
    string CounterName,
    string Status,
    DateTime StartedAt,
    IReadOnlyList<AvailableCounterAssignedServiceDto> AssignedServices);

public record ResetPasswordRequest(string NewPassword);

public record StaffListQuery(string? Search, string? Role, bool? IsActive, int Page = 1, int PageSize = 50);

public record AdminStaffListItemDto(
    int Id,
    string FullName,
    string Username,
    string Email,
    string Role,
    bool IsActive,
    string? ActiveCounterName,
    string? SessionStatus,
    int ServedToday);

public record CreateStaffRequest(
    string FullName,
    string Username,
    string Email,
    string Password,
    string Role,
    bool IsActive);

public record UpdateStaffRequest(
    string FullName,
    string Username,
    string Email,
    string Role,
    bool IsActive);

public record StaffManagementSummaryDto(
    int TotalStaff,
    int ActiveStaff,
    int OnlineNow,
    int AdminUsers);

public record PatchStaffStatusRequest(bool IsActive);
