using Microsoft.EntityFrameworkCore;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;
using SmartQ.Domain.Entities;
using SmartQ.Domain.Enums;
using SmartQ.Infrastructure.Persistence;

namespace SmartQ.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly SmartQDbContext _db;
    private readonly IQueueNotificationService _notify;

    public TokenService(SmartQDbContext db, IQueueNotificationService notify)
    {
        _db = db;
        _notify = notify;
    }

    public async Task<GenerateTokenResponse> GenerateTokenAsync(GenerateTokenRequest request, CancellationToken ct = default)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var language = await _db.Languages.FindAsync([request.LanguageId], ct)
                    ?? throw new InvalidOperationException("Language not found.");
                var service = await _db.Services.FindAsync([request.ServiceId], ct)
                    ?? throw new InvalidOperationException("Service not found.");
                var sub = await _db.SubServices.FindAsync([request.SubServiceId], ct)
                    ?? throw new InvalidOperationException("Sub-service not found.");

                if (!sub.IsActive || !service.IsActive)
                    throw new InvalidOperationException("Service is not available.");

                var sequenceDate = DateOnly.FromDateTime(DateTime.Today);
                var (seqNo, tokenNo) = await TokenSequenceHelper.NextAsync(_db, sub.Id, sub.TokenPrefix, ct);

                var waitingBefore = await _db.Tokens.CountAsync(
                    t => t.SubServiceId == sub.Id && t.Status == TokenStatus.WAITING, ct);

                var token = new Token
                {
                    TokenNo = tokenNo,
                    TokenPrefix = sub.TokenPrefix,
                    SequenceNo = seqNo,
                    SequenceDate = sequenceDate,
                    LanguageId = language.Id,
                    ServiceId = service.Id,
                    SubServiceId = sub.Id,
                    Status = TokenStatus.WAITING,
                    Priority = TokenPriority.STANDARD,
                    CreatedAt = DateTime.Now,
                    QueuedAt = DateTime.Now,
                    EstimatedWaitMinutes = sub.EstimatedServiceMinutes + waitingBefore * 2
                };
                _db.Tokens.Add(token);
                _db.TokenStatusHistories.Add(new TokenStatusHistory
                {
                    Token = token,
                    OldStatus = null,
                    NewStatus = TokenStatus.WAITING,
                    ChangedAt = DateTime.Now,
                    Remarks = "Token generated"
                });
                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                var (serviceName, subName) = await GetTranslatedNamesAsync(service.Id, sub.Id, language.Id, ct);
                var response = new GenerateTokenResponse(
                    token.Id, token.TokenNo, serviceName, subName,
                    token.EstimatedWaitMinutes, waitingBefore, token.CreatedAt);

                await _notify.TokenGeneratedAsync(response, ct);
                await _notify.QueueUpdatedAsync(new { token.Id }, ct);
                await _notify.DisplayUpdatedAsync(await BuildDisplayPayload(ct), ct);

                return response;
            }
            catch (DbUpdateException ex) when (attempt < maxAttempts && IsTokenNumberConflict(ex))
            {
                await tx.RollbackAsync(ct);
                _db.ChangeTracker.Clear();
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique token number. Please try again.");
    }

    public async Task<TokenDetailDto?> GetTokenAsync(int id, CancellationToken ct = default)
    {
        var token = await _db.Tokens.AsNoTracking()
            .Include(t => t.Service)
            .Include(t => t.SubService)
            .Include(t => t.Counter)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
        return token == null ? null : await MapDetailAsync(token, ct);
    }

    public async Task<TokenDetailDto?> RecallTokenAsync(int tokenId, CancellationToken ct = default)
    {
        var token = await _db.Tokens.Include(t => t.Counter).Include(t => t.Service).Include(t => t.SubService)
            .FirstOrDefaultAsync(t => t.Id == tokenId, ct);
        if (token == null || token.Status is not (TokenStatus.CALLED or TokenStatus.SERVING)) return null;

        await AddHistory(token, token.Status, TokenStatus.CALLED, ct);
        token.Status = TokenStatus.CALLED;
        token.CalledAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);

        var dto = MapDetail(token);
        await _notify.TokenRecalledAsync(dto, ct);
        await _notify.DisplayUpdatedAsync(await BuildDisplayPayload(ct), ct);
        return dto;
    }

    public async Task<TokenDetailDto?> StartTokenAsync(int tokenId, CancellationToken ct = default)
    {
        var token = await _db.Tokens.Include(t => t.Counter).Include(t => t.Service).Include(t => t.SubService)
            .FirstOrDefaultAsync(t => t.Id == tokenId, ct);
        if (token == null || token.Status != TokenStatus.CALLED) return null;

        await AddHistory(token, token.Status, TokenStatus.SERVING, ct);
        token.Status = TokenStatus.SERVING;
        token.StartedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);

        var dto = MapDetail(token);
        await _notify.TokenStartedAsync(dto, ct);
        return dto;
    }

    public async Task<TokenDetailDto?> CompleteTokenAsync(int tokenId, CancellationToken ct = default)
    {
        var token = await _db.Tokens.Include(t => t.Counter).Include(t => t.Service).Include(t => t.SubService)
            .FirstOrDefaultAsync(t => t.Id == tokenId, ct);
        if (token == null) return null;

        await AddHistory(token, token.Status, TokenStatus.COMPLETED, ct);
        token.Status = TokenStatus.COMPLETED;
        token.CompletedAt = DateTime.Now;
        if (token.CounterId.HasValue)
        {
            var counter = await _db.Counters.FindAsync([token.CounterId.Value], ct);
            if (counter != null) counter.Status = CounterStatus.AVAILABLE;
        }
        await _db.SaveChangesAsync(ct);

        var dto = MapDetail(token);
        await _notify.TokenCompletedAsync(dto, ct);
        await _notify.QueueUpdatedAsync(new { token.Id }, ct);
        await _notify.DisplayUpdatedAsync(await BuildDisplayPayload(ct), ct);
        return dto;
    }

    public async Task<TokenDetailDto?> SkipTokenAsync(int tokenId, CancellationToken ct = default)
    {
        var token = await _db.Tokens.Include(t => t.Counter).Include(t => t.Service).Include(t => t.SubService)
            .FirstOrDefaultAsync(t => t.Id == tokenId, ct);
        if (token == null) return null;

        await AddHistory(token, token.Status, TokenStatus.SKIPPED, ct);
        token.Status = TokenStatus.SKIPPED;
        token.SkippedAt = DateTime.Now;
        if (token.CounterId.HasValue)
        {
            var counter = await _db.Counters.FindAsync([token.CounterId.Value], ct);
            if (counter != null) counter.Status = CounterStatus.AVAILABLE;
        }
        await _db.SaveChangesAsync(ct);

        var dto = MapDetail(token);
        await _notify.TokenSkippedAsync(dto, ct);
        await _notify.QueueUpdatedAsync(new { token.Id }, ct);
        await _notify.DisplayUpdatedAsync(await BuildDisplayPayload(ct), ct);
        return dto;
    }

    public async Task<TokenDetailDto?> TransferTokenAsync(int tokenId, int targetCounterId, CancellationToken ct = default)
    {
        var token = await _db.Tokens.Include(t => t.Counter).Include(t => t.Service).Include(t => t.SubService)
            .FirstOrDefaultAsync(t => t.Id == tokenId, ct);
        if (token == null) return null;
        var oldTokenNo = token.TokenNo;

        await AddHistory(token, token.Status, TokenStatus.TRANSFERRED, ct, targetCounterId);
        token.Status = TokenStatus.WAITING;
        token.CounterId = null;
        token.CalledAt = null;
        token.QueuedAt = DateTime.Now;
        token.TransferCount += 1;
        token.LastTransferredAt = DateTime.Now;
        token.TransferredFromTokenNo = token.TransferCount > 1 && !string.IsNullOrWhiteSpace(token.TransferredFromTokenNo)
            ? token.TransferredFromTokenNo
            : oldTokenNo;
        await _db.SaveChangesAsync(ct);
        return MapDetail(token);
    }

    private async Task AddHistory(Token token, TokenStatus oldStatus, TokenStatus newStatus, CancellationToken ct, int? counterId = null)
    {
        _db.TokenStatusHistories.Add(new TokenStatusHistory
        {
            TokenId = token.Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            CounterId = counterId ?? token.CounterId,
            ChangedAt = DateTime.Now
        });
    }

    private async Task<TokenDetailDto> MapDetailAsync(Token t, CancellationToken ct)
    {
        var (serviceName, subName) = await GetTranslatedNamesAsync(t.ServiceId, t.SubServiceId, t.LanguageId, ct);
        return new TokenDetailDto(
            t.Id, t.TokenNo, serviceName, subName,
            t.Status.ToString(), t.Priority.ToString(), t.CounterId,
            t.Counter?.CounterName, t.EstimatedWaitMinutes, t.CreatedAt, t.CalledAt);
    }

    private TokenDetailDto MapDetail(Token t) => new(
        t.Id, t.TokenNo, t.Service.Name, t.SubService.Name,
        t.Status.ToString(), t.Priority.ToString(), t.CounterId,
        t.Counter?.CounterName, t.EstimatedWaitMinutes, t.CreatedAt, t.CalledAt);

    private async Task<(string ServiceName, string SubServiceName)> GetTranslatedNamesAsync(
        int serviceId, int subServiceId, int languageId, CancellationToken ct)
    {
        var serviceTr = await _db.ServiceTranslations.AsNoTracking()
            .FirstOrDefaultAsync(t => t.ServiceId == serviceId && t.LanguageId == languageId, ct);
        var subTr = await _db.SubServiceTranslations.AsNoTracking()
            .FirstOrDefaultAsync(t => t.SubServiceId == subServiceId && t.LanguageId == languageId, ct);
        var service = await _db.Services.AsNoTracking().FirstAsync(s => s.Id == serviceId, ct);
        var sub = await _db.SubServices.AsNoTracking().FirstAsync(s => s.Id == subServiceId, ct);
        return (serviceTr?.Name ?? service.Name, subTr?.Name ?? sub.Name);
    }

    private async Task<int> GetSettingIntAsync(string key, int fallback, CancellationToken ct)
    {
        var s = await _db.SystemSettings.AsNoTracking().FirstOrDefaultAsync(x => x.SettingKey == key, ct);
        return s != null && int.TryParse(s.SettingValue, out var v) ? v : fallback;
    }

    private async Task<object> BuildDisplayPayload(CancellationToken ct)
    {
        var display = new DisplayService(_db);
        return await display.GetDisplayBoardAsync(ct);
    }

    private static bool IsTokenNumberConflict(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("IX_Tokens_SubServiceId_SequenceDate_SequenceNo", StringComparison.OrdinalIgnoreCase)
            || message.Contains("IX_Tokens_TokenNo", StringComparison.OrdinalIgnoreCase);
    }
}
