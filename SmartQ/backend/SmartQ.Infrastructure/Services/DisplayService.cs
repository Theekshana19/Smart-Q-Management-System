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
            .Include(t => t.SubService)
            .Where(t => t.Status == TokenStatus.CALLED || t.Status == TokenStatus.SERVING)
            .OrderByDescending(t => t.CalledAt)
            .FirstOrDefaultAsync(ct);

        if (token?.Counter == null)
        {
            return null;
        }

        var latestTransferRemark = await GetLatestTransferRemarkAsync(token.Id, ct);
        return new NowServingDto(
            FormatDisplayTokenNo(token),
            token.Counter.CounterName,
            token.Counter.CounterNo,
            FormatDisplayServiceName(token, latestTransferRemark),
            token.CalledAt);
    }

    public async Task<IReadOnlyList<RecentlyCalledDto>> GetRecentlyCalledAsync(CancellationToken ct = default)
    {
        var count = await GetSettingIntAsync("DISPLAY_RECENT_CALL_COUNT", 3, ct);
        var tokens = await _db.Tokens.AsNoTracking()
            .Include(t => t.Counter)
            .Include(t => t.Service)
            .Include(t => t.SubService)
            .Where(t => t.CalledAt != null && t.Status != TokenStatus.WAITING)
            .OrderByDescending(t => t.CalledAt)
            .Take(count)
            .ToListAsync(ct);

        var tokenIds = tokens.Select(t => t.Id).ToList();
        var latestTransferRemarks = tokenIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.TokenStatusHistories.AsNoTracking()
                .Where(h => tokenIds.Contains(h.TokenId) && h.NewStatus == TokenStatus.TRANSFERRED && h.Remarks != null)
                .GroupBy(h => h.TokenId)
                .Select(g => new { TokenId = g.Key, Remark = g.OrderByDescending(x => x.ChangedAt).Select(x => x.Remarks!).FirstOrDefault()! })
                .ToDictionaryAsync(x => x.TokenId, x => x.Remark, ct);

        return tokens.Select(t => new RecentlyCalledDto(
            FormatDisplayTokenNo(t),
            t.Counter?.CounterName ?? "-",
            t.Counter?.CounterNo ?? "-",
            FormatDisplayServiceName(t, latestTransferRemarks.TryGetValue(t.Id, out var remark) ? remark : null),
            t.CalledAt!.Value)).ToList();
    }

    public async Task<WaitingQueueDto> GetWaitingQueueAsync(CancellationToken ct = default)
    {
        var count = await GetSettingIntAsync("DISPLAY_WAITING_QUEUE_COUNT", 5, ct);
        var items = await _db.Tokens.AsNoTracking()
            .Include(t => t.Service)
            .Include(t => t.SubService)
            .Where(t => t.Status == TokenStatus.WAITING)
            // Always keep transferred tokens behind non-transferred tokens in public queue.
            .OrderBy(t => (t.TransferCount > 0 || t.TransferredFromTokenNo != null) ? 1 : 0)
            .ThenBy(t => t.QueuedAt ?? t.CreatedAt)
            .ThenBy(t => t.CreatedAt)
            .Take(count)
            .ToListAsync(ct);

        var tokenIds = items.Select(i => i.Id).ToList();
        var latestTransferRemarks = tokenIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.TokenStatusHistories.AsNoTracking()
                .Where(h => tokenIds.Contains(h.TokenId) && h.NewStatus == TokenStatus.TRANSFERRED && h.Remarks != null)
                .GroupBy(h => h.TokenId)
                .Select(g => new { TokenId = g.Key, Remark = g.OrderByDescending(x => x.ChangedAt).Select(x => x.Remarks!).FirstOrDefault()! })
                .ToDictionaryAsync(x => x.TokenId, x => x.Remark, ct);

        var avgWait = items.Count > 0
            ? (int)items.Average(i => (DateTime.Now - i.CreatedAt).TotalMinutes) + 5
            : await GetSettingIntAsync("DEFAULT_ESTIMATED_WAIT_MINUTES", 8, ct);

        return new WaitingQueueDto(avgWait, items.Select(i => new WaitingQueueItemDto(
            FormatDisplayTokenNo(i),
            i.Service.Name,
            FormatDisplayServiceName(i, latestTransferRemarks.TryGetValue(i.Id, out var remark) ? remark : null),
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

    private async Task<string?> GetLatestTransferRemarkAsync(int tokenId, CancellationToken ct) =>
        await _db.TokenStatusHistories.AsNoTracking()
            .Where(h => h.TokenId == tokenId && h.NewStatus == TokenStatus.TRANSFERRED && h.Remarks != null)
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => h.Remarks)
            .FirstOrDefaultAsync(ct);

    private static string FormatDisplayTokenNo(Domain.Entities.Token token)
    {
        if (token.TransferCount <= 0)
        {
            return token.TokenNo;
        }

        if (!string.IsNullOrWhiteSpace(token.TransferredFromTokenNo))
        {
            return $"{token.TransferredFromTokenNo} ({token.TokenNo}(T))";
        }

        return $"{token.TokenNo} (T)";
    }

    private static string FormatDisplayServiceName(Domain.Entities.Token token, string? latestTransferRemark)
    {
        if (token.TransferCount <= 0 || string.IsNullOrWhiteSpace(latestTransferRemark))
        {
            return token.SubService.Name;
        }

        const string prefix = "Transfer: ";
        if (latestTransferRemark.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return latestTransferRemark[prefix.Length..].Trim();
        }

        return token.SubService.Name;
    }
}
