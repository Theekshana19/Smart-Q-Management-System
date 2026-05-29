using SmartQ.Application.DTOs;

namespace SmartQ.Application.Interfaces;

public interface IDisplayService
{
    Task<NowServingDto?> GetNowServingAsync(CancellationToken ct = default);
    Task<IReadOnlyList<RecentlyCalledDto>> GetRecentlyCalledAsync(CancellationToken ct = default);
    Task<WaitingQueueDto> GetWaitingQueueAsync(CancellationToken ct = default);
    Task<DisplayBoardDto> GetDisplayBoardAsync(CancellationToken ct = default);
    Task<VoiceTemplateDto?> GetVoiceTemplateAsync(string eventType, string languageCode, CancellationToken ct = default);
}
