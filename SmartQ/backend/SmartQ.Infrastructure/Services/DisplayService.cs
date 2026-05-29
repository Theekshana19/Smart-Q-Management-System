using Microsoft.EntityFrameworkCore;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;
using SmartQ.Domain.Enums;
using SmartQ.Infrastructure.Persistence;

namespace SmartQ.Infrastructure.Services;

public class DisplayService : IDisplayService
{
    private readonly SmartQDbContext _db;

    public DisplayService(SmartQDbContext db) => _db = db;

    public async Task<NowServingDto?> GetNowServingAsync(CancellationToken ct = default)
    {
        var token = await _db.Tokens.AsNoTracking()
            .Include(t => t.Counter)
            .Include(t => t.Service)
            .Where(t => t.Status == TokenStatus.CALLED || t.Status == TokenStatus.SERVING)
            .OrderByDescending(t => t.CalledAt)
            .FirstOrDefaultAsync(ct);

        return token?.Counter == null ? null : new NowServingDto(
            token.TokenNo, token.Counter.CounterName, token.Counter.CounterNo,
            token.Service.Name, token.CalledAt);
    }

    public async Task<IReadOnlyList<RecentlyCalledDto>> GetRecentlyCalledAsync(CancellationToken ct = default)
    {
        var count = await GetSettingIntAsync("DISPLAY_RECENT_CALL_COUNT", 3, ct);
        var tokens = await _db.Tokens.AsNoTracking()
            .Include(t => t.Counter)
            .Include(t => t.Service)
            .Where(t => t.CalledAt != null && t.Status != TokenStatus.WAITING)
            .OrderByDescending(t => t.CalledAt)
            .Take(count)
            .ToListAsync(ct);

        return tokens.Select(t => new RecentlyCalledDto(
            t.TokenNo, t.Counter?.CounterName ?? "-", t.Counter?.CounterNo ?? "-",
            t.Service.Name, t.CalledAt!.Value)).ToList();
    }

    public async Task<WaitingQueueDto> GetWaitingQueueAsync(CancellationToken ct = default)
    {
        var count = await GetSettingIntAsync("DISPLAY_WAITING_QUEUE_COUNT", 5, ct);
        var items = await _db.Tokens.AsNoTracking()
            .Include(t => t.Service)
            .Include(t => t.SubService)
            .Where(t => t.Status == TokenStatus.WAITING)
            .OrderBy(t => t.CreatedAt)
            .Take(count)
            .ToListAsync(ct);

        var avgWait = items.Count > 0
            ? (int)items.Average(i => (DateTime.Now - i.CreatedAt).TotalMinutes) + 5
            : await GetSettingIntAsync("DEFAULT_ESTIMATED_WAIT_MINUTES", 8, ct);

        return new WaitingQueueDto(avgWait, items.Select(i => new WaitingQueueItemDto(
            i.TokenNo, i.Service.Name, i.SubService.Name,
            (int)(DateTime.Now - i.CreatedAt).TotalMinutes)).ToList());
    }

    public async Task<DisplayBoardDto> GetDisplayBoardAsync(CancellationToken ct = default)
    {
        var now = await GetNowServingAsync(ct);
        var recent = await GetRecentlyCalledAsync(ct);
        var waiting = await GetWaitingQueueAsync(ct);
        var ticker = await _db.DisplayMessages.AsNoTracking()
            .Where(m => m.IsActive && m.MessageKey.StartsWith("TICKER"))
            .OrderBy(m => m.DisplayOrder)
            .Select(m => m.MessageText)
            .ToListAsync(ct);

        return new DisplayBoardDto(now, recent, waiting, ticker);
    }

    public async Task<VoiceTemplateDto?> GetVoiceTemplateAsync(string eventType, string languageCode, CancellationToken ct = default)
    {
        var lang = await _db.Languages.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Code == languageCode, ct);
        if (lang == null) return null;

        var template = await _db.VoiceTemplates.AsNoTracking()
            .FirstOrDefaultAsync(v => v.LanguageId == lang.Id && v.EventType == eventType && v.IsActive, ct);

        return template == null ? null : new VoiceTemplateDto(template.EventType, template.TemplateText, languageCode);
    }

    private async Task<int> GetSettingIntAsync(string key, int fallback, CancellationToken ct)
    {
        var s = await _db.SystemSettings.AsNoTracking().FirstOrDefaultAsync(x => x.SettingKey == key, ct);
        return s != null && int.TryParse(s.SettingValue, out var v) ? v : fallback;
    }
}
