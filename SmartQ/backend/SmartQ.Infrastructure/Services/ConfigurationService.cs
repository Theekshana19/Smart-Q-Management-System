using Microsoft.EntityFrameworkCore;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;
using SmartQ.Domain.Entities;
using SmartQ.Infrastructure.Persistence;

namespace SmartQ.Infrastructure.Services;

public class ConfigurationService : IConfigurationService
{
    private static readonly string[] PublicSettingKeys =
    [
        "BRANCH_NAME", "BANK_NAME", "ENABLE_VOICE_ANNOUNCEMENT", "ENABLE_PRINT_TOKEN",
        "DISPLAY_RECENT_CALL_COUNT", "DISPLAY_WAITING_QUEUE_COUNT", "KIOSK_AUTO_RETURN_SECONDS"
    ];

    private readonly SmartQDbContext _db;

    public ConfigurationService(SmartQDbContext db) => _db = db;

    // ─── Languages ───────────────────────────────────────────────────────────

    public async Task<LanguageManagementSummaryDto> GetLanguageManagementSummaryAsync(CancellationToken ct = default)
    {
        var total = await _db.Languages.CountAsync(ct);
        var active = await _db.Languages.CountAsync(l => l.IsActive, ct);
        var defaultCode = await _db.Languages.AsNoTracking()
            .Where(l => l.IsDefault)
            .Select(l => l.Code)
            .FirstOrDefaultAsync(ct);
        return new LanguageManagementSummaryDto(total, active, defaultCode);
    }

    public async Task<IReadOnlyList<AdminLanguageDto>> GetLanguagesAsync(CancellationToken ct = default) =>
        await _db.Languages.AsNoTracking()
            .OrderBy(l => l.DisplayOrder).ThenBy(l => l.Code)
            .Select(l => new AdminLanguageDto(l.Id, l.Code, l.Name, l.NativeName, l.IsDefault, l.IsActive, l.DisplayOrder))
            .ToListAsync(ct);

    public async Task<AdminLanguageDto> CreateLanguageAsync(UpsertLanguageRequest request, CancellationToken ct = default)
    {
        ValidateLanguageRequest(request);
        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.Languages.AnyAsync(l => l.Code == code, ct))
            throw new InvalidOperationException($"Language code '{code}' already exists.");

        var now = DateTime.UtcNow;
        if (request.IsDefault)
            await ClearDefaultLanguageAsync(ct);

        var lang = new Language
        {
            Code = code,
            Name = request.Name.Trim(),
            NativeName = request.NativeName.Trim(),
            IsDefault = request.IsDefault,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.Languages.Add(lang);
        await _db.SaveChangesAsync(ct);
        return MapLanguage(lang);
    }

    public async Task<AdminLanguageDto?> UpdateLanguageAsync(int id, UpsertLanguageRequest request, CancellationToken ct = default)
    {
        ValidateLanguageRequest(request);
        var lang = await _db.Languages.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (lang == null) return null;

        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.Languages.AnyAsync(l => l.Code == code && l.Id != id, ct))
            throw new InvalidOperationException($"Language code '{code}' already exists.");

        if (request.IsDefault && !lang.IsDefault)
            await ClearDefaultLanguageAsync(ct);

        lang.Code = code;
        lang.Name = request.Name.Trim();
        lang.NativeName = request.NativeName.Trim();
        lang.IsDefault = request.IsDefault;
        lang.IsActive = request.IsActive;
        lang.DisplayOrder = request.DisplayOrder;
        lang.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return MapLanguage(lang);
    }

    public async Task<AdminLanguageDto?> PatchLanguageStatusAsync(int id, PatchStatusRequest request, CancellationToken ct = default)
    {
        var lang = await _db.Languages.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (lang == null) return null;
        if (!request.IsActive && lang.IsDefault)
            throw new InvalidOperationException("Cannot deactivate the default language. Set another language as default first.");
        lang.IsActive = request.IsActive;
        lang.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return MapLanguage(lang);
    }

    public async Task<bool> DeleteLanguageAsync(int id, CancellationToken ct = default)
    {
        var lang = await _db.Languages.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (lang == null) return false;

        if (await IsLanguageInUseAsync(id, ct))
        {
            lang.IsActive = false;
            lang.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        _db.Languages.Remove(lang);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ─── Settings ────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<AdminSystemSettingDto>> GetAdminSettingsAsync(SettingListQuery query, CancellationToken ct = default)
    {
        var q = _db.SystemSettings.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(s => s.SettingKey.Contains(term) || s.Description.Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(query.DataType))
            q = q.Where(s => s.DataType == query.DataType.Trim());

        return await q.OrderBy(s => s.SettingKey)
            .Select(s => new AdminSystemSettingDto(s.Id, s.SettingKey, s.SettingValue, s.DataType, s.Description, s.IsActive))
            .ToListAsync(ct);
    }

    public async Task<AdminSystemSettingDto?> GetSettingByIdAsync(int id, CancellationToken ct = default) =>
        await _db.SystemSettings.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new AdminSystemSettingDto(s.Id, s.SettingKey, s.SettingValue, s.DataType, s.Description, s.IsActive))
            .FirstOrDefaultAsync(ct);

    public async Task<AdminSystemSettingDto?> UpdateAdminSettingAsync(int id, UpdateAdminSettingRequest request, CancellationToken ct = default)
    {
        var s = await _db.SystemSettings.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s == null) return null;

        ValidateSettingValue(s.DataType, request.SettingValue);
        s.SettingValue = request.SettingValue.Trim();
        if (request.Description != null) s.Description = request.Description.Trim();
        if (request.IsActive.HasValue) s.IsActive = request.IsActive.Value;
        await _db.SaveChangesAsync(ct);
        return new AdminSystemSettingDto(s.Id, s.SettingKey, s.SettingValue, s.DataType, s.Description, s.IsActive);
    }

    public async Task<IReadOnlyList<PublicSettingDto>> GetPublicSettingsAsync(CancellationToken ct = default)
    {
        var settings = await _db.SystemSettings.AsNoTracking()
            .Where(s => s.IsActive && PublicSettingKeys.Contains(s.SettingKey))
            .ToListAsync(ct);

        return PublicSettingKeys
            .Select(key => settings.FirstOrDefault(s => s.SettingKey == key))
            .Where(s => s != null)
            .Select(s => new PublicSettingDto(s!.SettingKey, s.SettingValue))
            .ToList();
    }

    // ─── Display Messages ──────────────────────────────────────────────────────

    public async Task<IReadOnlyList<AdminDisplayMessageDto>> GetDisplayMessagesAsync(DisplayMessageListQuery query, CancellationToken ct = default)
    {
        var q = _db.DisplayMessages.AsNoTracking().AsQueryable();
        if (query.LanguageId.HasValue) q = q.Where(m => m.LanguageId == query.LanguageId);
        if (!string.IsNullOrWhiteSpace(query.MessageKey))
        {
            var term = query.MessageKey.Trim();
            q = q.Where(m => m.MessageKey.Contains(term));
        }
        if (query.IsActive.HasValue) q = q.Where(m => m.IsActive == query.IsActive);

        return await q.OrderBy(m => m.MessageKey).ThenBy(m => m.DisplayOrder)
            .Select(m => new AdminDisplayMessageDto(
                m.Id, m.LanguageId, m.Language != null ? m.Language.Code : null,
                m.MessageKey, m.MessageText, m.IsActive, m.DisplayOrder))
            .ToListAsync(ct);
    }

    public async Task<AdminDisplayMessageDto> CreateDisplayMessageAsync(UpsertDisplayMessageRequest request, CancellationToken ct = default)
    {
        ValidateDisplayMessageRequest(request);
        await EnsureUniqueMessageKeyAsync(request.MessageKey.Trim(), request.LanguageId, null, ct);
        if (request.LanguageId.HasValue && !await _db.Languages.AnyAsync(l => l.Id == request.LanguageId, ct))
            throw new InvalidOperationException("Language does not exist.");

        var msg = new DisplayMessage
        {
            LanguageId = request.LanguageId,
            MessageKey = request.MessageKey.Trim(),
            MessageText = request.MessageText.Trim(),
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder
        };
        _db.DisplayMessages.Add(msg);
        await _db.SaveChangesAsync(ct);
        return await GetDisplayMessageDtoAsync(msg.Id, ct) ?? throw new InvalidOperationException("Failed to load created message.");
    }

    public async Task<AdminDisplayMessageDto?> UpdateDisplayMessageAsync(int id, UpsertDisplayMessageRequest request, CancellationToken ct = default)
    {
        ValidateDisplayMessageRequest(request);
        var msg = await _db.DisplayMessages.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (msg == null) return null;

        await EnsureUniqueMessageKeyAsync(request.MessageKey.Trim(), request.LanguageId, id, ct);
        if (request.LanguageId.HasValue && !await _db.Languages.AnyAsync(l => l.Id == request.LanguageId, ct))
            throw new InvalidOperationException("Language does not exist.");

        msg.LanguageId = request.LanguageId;
        msg.MessageKey = request.MessageKey.Trim();
        msg.MessageText = request.MessageText.Trim();
        msg.IsActive = request.IsActive;
        msg.DisplayOrder = request.DisplayOrder;
        await _db.SaveChangesAsync(ct);
        return await GetDisplayMessageDtoAsync(id, ct);
    }

    public async Task<AdminDisplayMessageDto?> PatchDisplayMessageStatusAsync(int id, PatchStatusRequest request, CancellationToken ct = default)
    {
        var msg = await _db.DisplayMessages.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (msg == null) return null;
        msg.IsActive = request.IsActive;
        await _db.SaveChangesAsync(ct);
        return await GetDisplayMessageDtoAsync(id, ct);
    }

    public async Task<bool> DeleteDisplayMessageAsync(int id, CancellationToken ct = default)
    {
        var msg = await _db.DisplayMessages.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (msg == null) return false;
        msg.IsActive = false;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<PublicDisplayMessageDto>> GetPublicDisplayMessagesAsync(string languageCode, CancellationToken ct = default)
    {
        var code = (languageCode ?? "EN").Trim().ToUpperInvariant();
        var langId = await _db.Languages.AsNoTracking()
            .Where(l => l.Code == code && l.IsActive)
            .Select(l => (int?)l.Id)
            .FirstOrDefaultAsync(ct);

        var messages = await _db.DisplayMessages.AsNoTracking()
            .Where(m => m.IsActive && (m.LanguageId == null || m.LanguageId == langId))
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new { m.MessageKey, m.MessageText, m.LanguageId })
            .ToListAsync(ct);

        return messages
            .GroupBy(m => m.MessageKey)
            .Select(g => g.OrderByDescending(x => x.LanguageId.HasValue).First())
            .Select(m => new PublicDisplayMessageDto(m.MessageKey, m.MessageText))
            .ToList();
    }

    // ─── Voice Templates ─────────────────────────────────────────────────────

    public async Task<IReadOnlyList<AdminVoiceTemplateDto>> GetVoiceTemplatesAsync(VoiceTemplateListQuery query, CancellationToken ct = default)
    {
        var q = _db.VoiceTemplates.AsNoTracking().AsQueryable();
        if (query.LanguageId.HasValue) q = q.Where(v => v.LanguageId == query.LanguageId);
        if (!string.IsNullOrWhiteSpace(query.EventType))
            q = q.Where(v => v.EventType == query.EventType.Trim().ToUpperInvariant());
        if (query.IsActive.HasValue) q = q.Where(v => v.IsActive == query.IsActive);

        return await q.OrderBy(v => v.Language.Code).ThenBy(v => v.EventType)
            .Select(v => new AdminVoiceTemplateDto(v.Id, v.LanguageId, v.Language.Code, v.EventType, v.TemplateText, v.IsActive))
            .ToListAsync(ct);
    }

    public async Task<AdminVoiceTemplateDto> CreateVoiceTemplateAsync(UpsertVoiceTemplateRequest request, CancellationToken ct = default)
    {
        ValidateVoiceTemplateRequest(request);
        if (!await _db.Languages.AnyAsync(l => l.Id == request.LanguageId, ct))
            throw new InvalidOperationException("Language does not exist.");

        var eventType = request.EventType.Trim().ToUpperInvariant();
        if (await _db.VoiceTemplates.AnyAsync(v => v.LanguageId == request.LanguageId && v.EventType == eventType, ct))
            throw new InvalidOperationException($"Template for '{eventType}' already exists for this language.");

        var tpl = new VoiceTemplate
        {
            LanguageId = request.LanguageId,
            EventType = eventType,
            TemplateText = request.TemplateText.Trim(),
            IsActive = request.IsActive
        };
        _db.VoiceTemplates.Add(tpl);
        await _db.SaveChangesAsync(ct);
        return await GetVoiceTemplateDtoAsync(tpl.Id, ct) ?? throw new InvalidOperationException("Failed to load created template.");
    }

    public async Task<AdminVoiceTemplateDto?> UpdateVoiceTemplateAsync(int id, UpsertVoiceTemplateRequest request, CancellationToken ct = default)
    {
        ValidateVoiceTemplateRequest(request);
        var tpl = await _db.VoiceTemplates.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (tpl == null) return null;

        var eventType = request.EventType.Trim().ToUpperInvariant();
        if (await _db.VoiceTemplates.AnyAsync(v => v.LanguageId == request.LanguageId && v.EventType == eventType && v.Id != id, ct))
            throw new InvalidOperationException($"Template for '{eventType}' already exists for this language.");

        tpl.LanguageId = request.LanguageId;
        tpl.EventType = eventType;
        tpl.TemplateText = request.TemplateText.Trim();
        tpl.IsActive = request.IsActive;
        await _db.SaveChangesAsync(ct);
        return await GetVoiceTemplateDtoAsync(id, ct);
    }

    public async Task<AdminVoiceTemplateDto?> PatchVoiceTemplateStatusAsync(int id, PatchStatusRequest request, CancellationToken ct = default)
    {
        var tpl = await _db.VoiceTemplates.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (tpl == null) return null;
        tpl.IsActive = request.IsActive;
        await _db.SaveChangesAsync(ct);
        return await GetVoiceTemplateDtoAsync(id, ct);
    }

    public async Task<bool> DeleteVoiceTemplateAsync(int id, CancellationToken ct = default)
    {
        var tpl = await _db.VoiceTemplates.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (tpl == null) return false;
        tpl.IsActive = false;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<VoiceTemplateDto?> GetRuntimeVoiceTemplateAsync(string languageCode, string eventType, CancellationToken ct = default)
    {
        var code = (languageCode ?? "EN").Trim().ToUpperInvariant();
        var evt = (eventType ?? "TOKEN_CALLED").Trim().ToUpperInvariant();
        var lang = await _db.Languages.AsNoTracking().FirstOrDefaultAsync(l => l.Code == code && l.IsActive, ct);
        if (lang == null) return null;

        var tpl = await _db.VoiceTemplates.AsNoTracking()
            .Where(v => v.LanguageId == lang.Id && v.EventType == evt && v.IsActive)
            .Select(v => new VoiceTemplateDto(v.EventType, v.TemplateText, lang.Code))
            .FirstOrDefaultAsync(ct);

        return tpl;
    }

    // ─── Translations ──────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ServiceTranslationDto>> GetServiceTranslationsAsync(int serviceId, CancellationToken ct = default)
    {
        if (!await _db.Services.AnyAsync(s => s.Id == serviceId, ct))
            throw new InvalidOperationException("Service does not exist.");

        return await _db.ServiceTranslations.AsNoTracking()
            .Where(t => t.ServiceId == serviceId)
            .Join(_db.Languages.AsNoTracking(), t => t.LanguageId, l => l.Id, (t, l) => new ServiceTranslationDto(t.LanguageId, l.Code, t.Name, t.Description))
            .OrderBy(t => t.LanguageCode)
            .ToListAsync(ct);
    }

    public async Task SaveServiceTranslationsAsync(int serviceId, IReadOnlyList<UpsertServiceTranslationRequest> items, CancellationToken ct = default)
    {
        if (!await _db.Services.AnyAsync(s => s.Id == serviceId, ct))
            throw new InvalidOperationException("Service does not exist.");

        var existing = await _db.ServiceTranslations.Where(t => t.ServiceId == serviceId).ToListAsync(ct);
        _db.ServiceTranslations.RemoveRange(existing);

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Name)) continue;
            _db.ServiceTranslations.Add(new ServiceTranslation
            {
                ServiceId = serviceId,
                LanguageId = item.LanguageId,
                Name = item.Name.Trim(),
                Description = item.Description?.Trim() ?? string.Empty
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SubServiceTranslationDto>> GetSubServiceTranslationsAsync(int subServiceId, CancellationToken ct = default)
    {
        if (!await _db.SubServices.AnyAsync(s => s.Id == subServiceId, ct))
            throw new InvalidOperationException("Sub-service does not exist.");

        return await _db.SubServiceTranslations.AsNoTracking()
            .Where(t => t.SubServiceId == subServiceId)
            .Join(_db.Languages.AsNoTracking(), t => t.LanguageId, l => l.Id, (t, l) => new SubServiceTranslationDto(t.LanguageId, l.Code, t.Name, t.Description))
            .OrderBy(t => t.LanguageCode)
            .ToListAsync(ct);
    }

    public async Task SaveSubServiceTranslationsAsync(int subServiceId, IReadOnlyList<UpsertSubServiceTranslationRequest> items, CancellationToken ct = default)
    {
        if (!await _db.SubServices.AnyAsync(s => s.Id == subServiceId, ct))
            throw new InvalidOperationException("Sub-service does not exist.");

        var existing = await _db.SubServiceTranslations.Where(t => t.SubServiceId == subServiceId).ToListAsync(ct);
        _db.SubServiceTranslations.RemoveRange(existing);

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Name)) continue;
            _db.SubServiceTranslations.Add(new SubServiceTranslation
            {
                SubServiceId = subServiceId,
                LanguageId = item.LanguageId,
                Name = item.Name.Trim(),
                Description = item.Description?.Trim() ?? string.Empty
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    // ─── Helpers ───────────────────────────────────────────────────────────────

    private static AdminLanguageDto MapLanguage(Language l) =>
        new(l.Id, l.Code, l.Name, l.NativeName, l.IsDefault, l.IsActive, l.DisplayOrder);

    private async Task ClearDefaultLanguageAsync(CancellationToken ct)
    {
        await _db.Languages.Where(l => l.IsDefault).ExecuteUpdateAsync(s => s.SetProperty(l => l.IsDefault, false), ct);
    }

    private async Task<bool> IsLanguageInUseAsync(int languageId, CancellationToken ct) =>
        await _db.Tokens.AnyAsync(t => t.LanguageId == languageId, ct)
        || await _db.ServiceTranslations.AnyAsync(t => t.LanguageId == languageId, ct)
        || await _db.SubServiceTranslations.AnyAsync(t => t.LanguageId == languageId, ct)
        || await _db.DisplayMessages.AnyAsync(m => m.LanguageId == languageId, ct)
        || await _db.VoiceTemplates.AnyAsync(v => v.LanguageId == languageId, ct);

    private async Task<AdminDisplayMessageDto?> GetDisplayMessageDtoAsync(int id, CancellationToken ct) =>
        await _db.DisplayMessages.AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new AdminDisplayMessageDto(
                m.Id, m.LanguageId, m.Language != null ? m.Language.Code : null,
                m.MessageKey, m.MessageText, m.IsActive, m.DisplayOrder))
            .FirstOrDefaultAsync(ct);

    private async Task<AdminVoiceTemplateDto?> GetVoiceTemplateDtoAsync(int id, CancellationToken ct) =>
        await _db.VoiceTemplates.AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => new AdminVoiceTemplateDto(v.Id, v.LanguageId, v.Language.Code, v.EventType, v.TemplateText, v.IsActive))
            .FirstOrDefaultAsync(ct);

    private async Task EnsureUniqueMessageKeyAsync(string key, int? languageId, int? excludeId, CancellationToken ct)
    {
        var exists = await _db.DisplayMessages.AnyAsync(m =>
            m.MessageKey == key && m.LanguageId == languageId && (!excludeId.HasValue || m.Id != excludeId), ct);
        if (exists)
            throw new InvalidOperationException($"Message key '{key}' already exists for this language scope.");
    }

    private static void ValidateLanguageRequest(UpsertLanguageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code)) throw new InvalidOperationException("Code is required.");
        if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Name is required.");
        if (string.IsNullOrWhiteSpace(request.NativeName)) throw new InvalidOperationException("Native name is required.");
        if (request.DisplayOrder < 0) throw new InvalidOperationException("Display order must be >= 0.");
    }

    private static void ValidateDisplayMessageRequest(UpsertDisplayMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MessageKey)) throw new InvalidOperationException("Message key is required.");
        if (string.IsNullOrWhiteSpace(request.MessageText)) throw new InvalidOperationException("Message text is required.");
        if (request.DisplayOrder < 0) throw new InvalidOperationException("Display order must be >= 0.");
    }

    private static void ValidateVoiceTemplateRequest(UpsertVoiceTemplateRequest request)
    {
        if (request.LanguageId <= 0) throw new InvalidOperationException("Language is required.");
        if (string.IsNullOrWhiteSpace(request.EventType)) throw new InvalidOperationException("Event type is required.");
        if (string.IsNullOrWhiteSpace(request.TemplateText)) throw new InvalidOperationException("Template text is required.");
    }

    private static void ValidateSettingValue(string dataType, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Setting value is required.");
        var type = dataType.Trim().ToUpperInvariant();
        switch (type)
        {
            case "BOOL":
            case "BOOLEAN":
                if (!bool.TryParse(value, out _))
                    throw new InvalidOperationException("Value must be true or false.");
                break;
            case "INT":
            case "NUMBER":
                if (!int.TryParse(value, out _))
                    throw new InvalidOperationException("Value must be a valid integer.");
                break;
            case "DECIMAL":
                if (!decimal.TryParse(value, out _))
                    throw new InvalidOperationException("Value must be a valid decimal number.");
                break;
            case "JSON":
                try { System.Text.Json.JsonDocument.Parse(value); }
                catch { throw new InvalidOperationException("Value must be valid JSON."); }
                break;
        }
    }
}
