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
