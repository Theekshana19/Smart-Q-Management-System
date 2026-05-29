namespace SmartQ.Application.DTOs;

public record DashboardSummaryDto(
    int ActiveTokens, double AvgWaitMinutes, int StaffOnline, int StaffTotal,
    double SatisfactionRate, int TokensToday, double WaitTrendMinutes,
    IReadOnlyList<HourlyFlowDto> HourlyFlow, IReadOnlyList<TokenDistributionDto> TokenDistribution,
    IReadOnlyList<ActivityItemDto> RecentActivity, IReadOnlyList<CounterStatusDto> CounterStatuses);

public record HourlyFlowDto(string Hour, int General, int Priority);
public record TokenDistributionDto(string Category, int Count, string Color);
public record ActivityItemDto(string Title, string Description, string Type, string TimeAgo);
public record CounterStatusDto(string CounterId, string StaffName, string Status, int LoadPercent, bool IsVip);

public record AdminServiceDto(
    int Id, string Code, string Name, string Description, string Icon,
    int DisplayOrder, bool IsActive, string Department, string Status,
    int DailyVolume, string AvgWaitTime);

public record ServiceManagementSummaryDto(
    int TotalServices, int ActiveNow, int TotalTokensToday, double AvgWaitMinutes);

public record UpsertServiceRequest(
    string Code, string Name, string Description, string Icon, int DisplayOrder, bool IsActive);

public record AdminSubServiceDto(
    int Id, int ServiceId, string ServiceName, string Code, string Name,
    string TokenPrefix, int EstimatedServiceMinutes, bool IsActive);

public record UpsertSubServiceRequest(
    int ServiceId, string Code, string Name, string Description, string TokenPrefix,
    string Icon, int EstimatedServiceMinutes, int DisplayOrder, bool IsActive);

public record AdminCounterDto(
    int Id, string CounterNo, string CounterName, string Status, bool IsActive,
    string? AssignedStaff, IReadOnlyList<string> AssignedServices);

public record UpsertCounterRequest(
    string CounterNo, string CounterName, string Status, bool IsActive, IReadOnlyList<int>? ServiceIds);

public record SystemSettingDto(int Id, string SettingKey, string SettingValue, string DataType, string Description);

public record UpdateSettingRequest(string SettingValue);

public record TokenHistoryFilter(
    DateTime? From, DateTime? To, int? ServiceId, int? CounterId, string? Status);

public record TokenHistoryReportDto(
    int TotalIssued, int Completed, double AvgWaitMinutes,
    IReadOnlyList<TokenHistoryRowDto> Rows);

public record TokenHistoryRowDto(
    int Id, string TokenNo, string ServiceName, string SubServiceName,
    string? CounterName, DateTime CreatedAt, DateTime? CalledAt, DateTime? CompletedAt, string Status);
