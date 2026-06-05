namespace SmartQ.Application.DTOs;

// ─── Shared ───────────────────────────────────────────────────────────────────

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);

public record AdminProfileDto(string FullName, string Role);

public record PatchStatusRequest(bool IsActive);

public record PatchCounterStatusRequest(string Status);

// ─── Dashboard ────────────────────────────────────────────────────────────────

public record DashboardSummaryDto(
    int ActiveTokens,
    double AvgWaitMinutes,
    int StaffOnline,
    int StaffTotal,
    double? SatisfactionRate,
    int TokensToday,
    double WaitTrendMinutes,
    double ActiveTokensTrendPercent,
    IReadOnlyList<HourlyFlowDto> HourlyFlow,
    IReadOnlyList<TokenDistributionDto> TokenDistribution,
    IReadOnlyList<ActivityItemDto> RecentActivity,
    IReadOnlyList<CounterStatusDto> CounterStatuses);

public record HourlyFlowDto(string Hour, int General, int Priority);
public record TokenDistributionDto(string Category, int Count, string Color);
public record ActivityItemDto(string Title, string Description, string Type, string TimeAgo);
public record CounterStatusDto(string CounterId, string StaffName, string Status, int LoadPercent, bool IsVip);

// ─── Service Master ───────────────────────────────────────────────────────────

public record ServiceListQuery(string? Search, bool? IsActive, int Page = 1, int PageSize = 50);

public record AdminServiceListItemDto(
    int Id,
    string Code,
    string Name,
    string Description,
    string Icon,
    int DisplayOrder,
    bool IsActive,
    int SubServiceCount,
    int AssignedCounterCount,
    int TokensToday,
    double AverageWaitMinutes);

public record ServiceManagementSummaryDto(
    int TotalServices,
    int ActiveNow,
    int TotalTokensToday,
    double AvgWaitMinutes);

public record UpsertServiceRequest(
    string Code,
    string Name,
    string Description,
    string Icon,
    int DisplayOrder,
    bool IsActive);

// ─── Sub-Service Master ───────────────────────────────────────────────────────

public record SubServiceListQuery(int? ServiceId, string? Search, bool? IsActive, int Page = 1, int PageSize = 50);

public record AdminSubServiceListItemDto(
    int Id,
    int ServiceId,
    string ServiceName,
    string Code,
    string Name,
    string Description,
    string TokenPrefix,
    string Icon,
    int EstimatedServiceMinutes,
    int DisplayOrder,
    bool IsActive,
    int TokensToday);

public record UpsertSubServiceRequest(
    int ServiceId,
    string Code,
    string Name,
    string Description,
    string TokenPrefix,
    string Icon,
    int EstimatedServiceMinutes,
    int DisplayOrder,
    bool IsActive);

// ─── Counter Master ───────────────────────────────────────────────────────────

public record AdminCounterListItemDto(
    int Id,
    string CounterNo,
    string CounterName,
    string Status,
    bool IsActive,
    IReadOnlyList<string> AssignedServices,
    string? ActiveStaffName,
    string? CurrentTokenNo,
    int TokensToday);

public record CounterManagementSummaryDto(
    int ActiveCounters,
    int TotalCounters,
    int StaffLive,
    int StaffCapacityPercent,
    string AvgServiceTime,
    int PendingTickets,
    string TrafficAlert);

public record CounterManagementCardDto(
    int Id,
    string CounterNo,
    string CounterName,
    string UnitName,
    string Status,
    string? StaffName,
    string? StaffRole,
    string? CurrentTicket,
    int ProgressPercent,
    string WaitTimeLabel,
    string? WaitLimitLabel,
    int TodayVolume,
    double? FeedbackScore,
    bool IsOffline,
    string? OfflineMessage);

public record CounterManagementDto(
    CounterManagementSummaryDto Summary,
    IReadOnlyList<CounterManagementCardDto> Counters);

public record UpsertCounterRequest(
    string CounterNo,
    string CounterName,
    string Status,
    bool IsActive);

// ─── Counter-Service Assignment ───────────────────────────────────────────────

public record CounterAssignmentListItemDto(
    int CounterId,
    string CounterNo,
    string CounterName,
    IReadOnlyList<int> AssignedServiceIds,
    IReadOnlyList<string> AssignedServiceNames);

public record AssignableServiceDto(
    int Id,
    string Code,
    string Name,
    bool IsActive,
    bool IsAssigned,
    IReadOnlyList<string> TokenPrefixes);

public record SaveCounterAssignmentRequest(int CounterId, IReadOnlyList<int> ServiceIds);

// ─── Settings ─────────────────────────────────────────────────────────────────

public record SystemSettingDto(int Id, string SettingKey, string SettingValue, string DataType, string Description);
public record UpdateSettingRequest(string SettingValue);

// ─── Reports ──────────────────────────────────────────────────────────────────

public record TokenHistoryFilter(
    DateTime? DateFrom,
    DateTime? DateTo,
    int? ServiceId,
    int? SubServiceId,
    int? CounterId,
    string? Status,
    int Page = 1,
    int PageSize = 50);

public record TokenHistoryRowDto(
    int Id,
    string TokenNo,
    string ServiceName,
    string SubServiceName,
    string? CounterName,
    DateTime CreatedAt,
    DateTime? CalledAt,
    DateTime? CompletedAt,
    double? WaitingMinutes,
    double? ServiceMinutes,
    string Status);

public record TokenHistorySummaryDto(
    int TotalTokens,
    int Completed,
    int Skipped,
    double AverageWaitMinutes);

public record TokenHistoryReportDto(
    IReadOnlyList<TokenHistoryRowDto> Items,
    int TotalCount,
    TokenHistorySummaryDto Summary,
    IReadOnlyList<TrafficDistributionPointDto> TrafficDistribution,
    string PeakHoursSummary,
    IReadOnlyList<OperationalInsightDto> Insights);

public record TrafficDistributionPointDto(string HourLabel, int Count, int Percent);
public record OperationalInsightDto(string Icon, string Text, string Tone);

// Legacy resource load (dashboard counter page)
public record ResourceLoadPointDto(string Label, int AllocatedPercent, int CapacityPercent);
public record CounterOptimizationDto(string Recommendation, bool CanApply);
public record CounterResourceLoadDto(
    IReadOnlyList<ResourceLoadPointDto> Loads,
    CounterOptimizationDto Optimization);
