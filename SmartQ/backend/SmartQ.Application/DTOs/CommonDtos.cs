namespace SmartQ.Application.DTOs;

public record LanguageDto(int Id, string Code, string Name, string NativeName, bool IsDefault);

public record ServiceDto(int Id, string Code, string Name, string Description, string Icon, int DisplayOrder);

public record SubServiceDto(
    int Id, int ServiceId, string Code, string Name, string Description,
    string TokenPrefix, string Icon, int EstimatedServiceMinutes,
    int WaitingCount, int EstimatedWaitMinutes);

public record KioskStatusDto(
    string BranchId, string BranchName, string KioskVersion,
    bool SystemOnline, int ActiveStaffCount, int AverageWaitMinutes);

public record GenerateTokenRequest(int LanguageId, int ServiceId, int SubServiceId);

public record GenerateTokenResponse(
    int Id, string TokenNo, string ServiceName, string SubServiceName,
    int EstimatedWaitMinutes, int WaitingBeforeYou, DateTime CreatedAt);

public record TokenDetailDto(
    int Id, string TokenNo, string ServiceName, string SubServiceName,
    string Status, string Priority, int? CounterId, string? CounterName,
    int EstimatedWaitMinutes, DateTime CreatedAt, DateTime? CalledAt);

public record CounterDto(int Id, string CounterNo, string CounterName, string Status, bool IsActive);

public record QueueTokenDto(
    int Id, string TokenNo, string ServiceName, string SubServiceName,
    string Status, string Priority, int WaitMinutes, DateTime CreatedAt);

public record CounterQueueDto(
    int CounterId, string CounterName, string CounterStatus,
    IReadOnlyList<string> AssignedServiceNames,
    TokenDetailDto? ActiveToken, TokenDetailDto? NextToken,
    IReadOnlyList<QueueTokenDto> WaitingTokens);

public record CallNextResponse(
    int TokenId, string TokenNo, string ServiceName, string SubServiceName,
    string CounterName, string CounterNo);

public record CallNextResultDto(bool Success, string Message, CallNextResponse? Data);

public record StaffConsoleSummaryDto(
    int WaitingCount, int ServedToday, int AvgWaitMinutes,
    CounterQueueDto Queue);

public record NowServingDto(string TokenNo, string CounterName, string CounterNo, string ServiceName, DateTime? CalledAt);

public record RecentlyCalledDto(string TokenNo, string CounterName, string CounterNo, string ServiceName, DateTime CalledAt);

public record WaitingQueueItemDto(string TokenNo, string ServiceName, string SubServiceName, int WaitMinutes);

public record WaitingQueueDto(int ExpectedWaitMinutes, IReadOnlyList<WaitingQueueItemDto> Items);

public record DisplayBoardDto(
    NowServingDto? NowServing,
    IReadOnlyList<RecentlyCalledDto> RecentlyCalled,
    WaitingQueueDto WaitingQueue,
    IReadOnlyList<string> TickerMessages);

public record VoiceTemplateDto(string EventType, string TemplateText, string LanguageCode);

public record StaffCounterContextDto(int Id, string CounterNo, string CounterName, string Status, string BranchName);
public record StaffUserContextDto(int Id, string FullName, string Role);
public record StaffAssignedServiceDto(int ServiceId, string Code, string Name, string Icon, IReadOnlyList<string> TokenPrefixes);
public record StaffConsoleContextDto(
    StaffCounterContextDto Counter,
    StaffUserContextDto? Staff,
    IReadOnlyList<StaffAssignedServiceDto> AssignedServices,
    bool SystemOnline,
    bool CallNextLockWhenActiveToken,
    IReadOnlyDictionary<string, string> DisplayMessages,
    DateTime CurrentTime);

public record StaffConsoleSummaryV2Dto(
    int Waiting,
    int ServedToday,
    int CompletedToday,
    int SkippedToday,
    int AvgWaitMinutes,
    string AvgServiceTime,
    string CurrentStatus,
    string QueuePressure);

public record StaffActiveSessionDto(
    int TokenId,
    string TokenNo,
    string Status,
    string ServiceName,
    string SubServiceName,
    string Priority,
    DateTime? CalledAt,
    DateTime? StartedAt,
    int ElapsedSeconds,
    int EstimatedServiceMinutes,
    bool DisplayedOnTv,
    bool VoiceAnnouncementSent);

public record StaffQueueItemDto(
    int TokenId,
    string TokenNo,
    string ServiceName,
    string SubServiceName,
    int WaitMinutes,
    string Priority,
    string Status,
    DateTime CreatedAt,
    int QueuePosition);

public record CallNextActionResultDto(bool HasToken, string Message, StaffActiveSessionDto? Token);
public record TokenActionResultDto(bool Success, string Message, StaffActiveSessionDto? Token);

public record StaffTransferTokenRequest(
    int TargetServiceId,
    int TargetSubServiceId,
    int? TargetCounterId,
    string? Reason);

public record StaffTokenHistoryItemDto(
    int TokenId,
    string TokenNo,
    string ServiceType,
    DateTime? CalledTime,
    string Duration,
    string Status);

public record HourlyServedPointDto(string HourLabel, int ServedCount);

public record HourlyTrafficPointDto(string HourLabel, int CashCount, int AccountCount, int LoanCount);

public record StaffTimelineItemDto(
    string EventType,
    string TokenNo,
    string Title,
    string Description,
    string? MetricLabel,
    string? MetricValue,
    DateTime Timestamp);

public record StaffPerformanceDto(
    int ServedToday,
    string AvgServiceTime,
    int Skipped,
    decimal CompletionRate,
    IReadOnlyList<HourlyServedPointDto> HourlyServed,
    IReadOnlyList<StaffTimelineItemDto> RecentTimeline,
    string OptimizationTip,
    string StaffName,
    string ReportDateLabel,
    string RangeLabel,
    string ServedLabel,
    int DailyTarget,
    decimal ServedProgressPercent,
    string ServedTrendLabel,
    string AvgServiceTimeTrendLabel,
    decimal AvgServiceProgressPercent,
    string AvgServiceHint,
    string CompletionTrendLabel,
    decimal CompletionProgressPercent,
    string CompletionHint,
    IReadOnlyList<HourlyTrafficPointDto> HourlyTraffic);

public record StaffNotificationItemDto(string Type, string Title, string Description, DateTime CreatedAt, bool IsNew);
public record StaffNotificationResponseDto(int NewCount, IReadOnlyList<StaffNotificationItemDto> Items);

public record TokenJourneyItemDto(
    string NewStatus,
    DateTime ChangedAt,
    string? Remarks,
    string Title,
    string Subtitle);

public record StaffTokenDetailsDto(
    int TokenId,
    string TokenNo,
    string Status,
    string ServiceType,
    string SubService,
    string PreferredLanguage,
    string Priority,
    DateTime CreatedTime,
    int WaitingMinutes,
    int QueuePosition,
    string? CustomerName,
    string? CustomerSubtitle,
    IReadOnlyList<TokenJourneyItemDto> Journey);

public record StaffTransferOptionDto(int Id, string Code, string Name);
public record StaffTransferSubServiceDto(int Id, int ServiceId, string Code, string Name);
public record StaffTransferCounterDto(int Id, string CounterNo, string CounterName, string Status);
public record StaffTransferOptionsDto(
    IReadOnlyList<StaffTransferOptionDto> Services,
    IReadOnlyList<StaffTransferSubServiceDto> SubServices,
    IReadOnlyList<StaffTransferCounterDto> Counters);

public record StaffDashboardCounterDto(
    int CounterId,
    string CounterName,
    string? StaffName,
    string Status,
    string? CurrentToken,
    int LoadPercent,
    string LoadLabel);

public record StaffDashboardCompositionDto(string Code, string Name, int WaitingCount);
public record StaffSystemStatusItemDto(string Key, string Title, string Value, string Level);

public record StaffDashboardDto(
    string BranchName,
    string HeroTitle,
    string HeroDescription,
    int SystemHealthPercent,
    string AvgWaitDisplay,
    int TokensToday,
    int StaffEfficiencyPercent,
    IReadOnlyList<StaffDashboardCounterDto> ActiveCounters,
    IReadOnlyList<StaffDashboardCompositionDto> QueueComposition,
    IReadOnlyList<StaffSystemStatusItemDto> SystemStatuses,
    StaffNotificationResponseDto Notifications,
    string LiveStreamTitle,
    string LiveStreamUrl,
    DateTime CurrentTime);

public record StaffMyCounterActiveDetailsDto(
    string TokenIdLabel,
    string CustomerLabel,
    string WaitTimeDisplay,
    int WaitMinutes);

public record StaffMyCounterUpcomingTokenDto(
    int TokenId,
    string TokenNo,
    string TokenPrefixBadge,
    string SubServiceName,
    int WaitMinutes);

public record StaffMyCounterEfficiencyDto(
    int EfficiencyPercent,
    string EfficiencyTrend,
    string BreakTimeDisplay,
    string SuccessRateDisplay,
    string ShiftEndsInDisplay);

public record StaffMyCounterDto(
    StaffConsoleContextDto Context,
    StaffConsoleSummaryV2Dto Summary,
    StaffActiveSessionDto? ActiveSession,
    StaffMyCounterActiveDetailsDto? ActiveDetails,
    IReadOnlyList<StaffMyCounterUpcomingTokenDto> UpcomingTokens,
    StaffPerformanceDto Performance,
    StaffMyCounterEfficiencyDto Efficiency,
    int QueuePressurePercent,
    string QueuePressureLabel);

public record StaffCounterStatusRequest(string Status);

public record StaffCounterStatusResultDto(bool Success, string Message, string Status);
