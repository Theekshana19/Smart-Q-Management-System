namespace SmartQ.Application.DTOs;

// ─── Languages ────────────────────────────────────────────────────────────────

public record LanguageManagementSummaryDto(int TotalLanguages, int ActiveLanguages, string? DefaultLanguageCode);

public record AdminLanguageDto(
    int Id,
    string Code,
    string Name,
    string NativeName,
    bool IsDefault,
    bool IsActive,
    int DisplayOrder);

public record UpsertLanguageRequest(
    string Code,
    string Name,
    string NativeName,
    bool IsDefault,
    bool IsActive,
    int DisplayOrder);

// ─── System Settings ────────────────────────────────────────────────────────────

public record AdminSystemSettingDto(
    int Id,
    string SettingKey,
    string SettingValue,
    string DataType,
    string Description,
    bool IsActive);

public record UpdateAdminSettingRequest(string SettingValue, string? Description = null, bool? IsActive = null);

public record SettingListQuery(string? Search, string? DataType);

public record PublicSettingDto(string Key, string Value);

// ─── Display Messages ─────────────────────────────────────────────────────────

public record DisplayMessageListQuery(int? LanguageId, string? MessageKey, bool? IsActive);

public record AdminDisplayMessageDto(
    int Id,
    int? LanguageId,
    string? LanguageCode,
    string MessageKey,
    string MessageText,
    bool IsActive,
    int DisplayOrder);

public record UpsertDisplayMessageRequest(
    int? LanguageId,
    string MessageKey,
    string MessageText,
    bool IsActive,
    int DisplayOrder);

public record PublicDisplayMessageDto(string MessageKey, string MessageText);

// ─── Voice Templates ──────────────────────────────────────────────────────────

public record VoiceTemplateListQuery(int? LanguageId, string? EventType, bool? IsActive);

public record AdminVoiceTemplateDto(
    int Id,
    int LanguageId,
    string LanguageCode,
    string EventType,
    string TemplateText,
    bool IsActive);

public record UpsertVoiceTemplateRequest(
    int LanguageId,
    string EventType,
    string TemplateText,
    bool IsActive);

// ─── Translations ─────────────────────────────────────────────────────────────

public record ServiceTranslationDto(int LanguageId, string LanguageCode, string Name, string Description);

public record UpsertServiceTranslationRequest(int LanguageId, string Name, string Description);

public record SubServiceTranslationDto(int LanguageId, string LanguageCode, string Name, string Description);

public record UpsertSubServiceTranslationRequest(int LanguageId, string Name, string Description);
