using SmartQ.Application.DTOs;

namespace SmartQ.Application.Interfaces;

public interface IConfigurationService
{
    Task<LanguageManagementSummaryDto> GetLanguageManagementSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AdminLanguageDto>> GetLanguagesAsync(CancellationToken ct = default);
    Task<AdminLanguageDto> CreateLanguageAsync(UpsertLanguageRequest request, CancellationToken ct = default);
    Task<AdminLanguageDto?> UpdateLanguageAsync(int id, UpsertLanguageRequest request, CancellationToken ct = default);
    Task<AdminLanguageDto?> PatchLanguageStatusAsync(int id, PatchStatusRequest request, CancellationToken ct = default);
    Task<bool> DeleteLanguageAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<AdminSystemSettingDto>> GetAdminSettingsAsync(SettingListQuery query, CancellationToken ct = default);
    Task<AdminSystemSettingDto?> GetSettingByIdAsync(int id, CancellationToken ct = default);
    Task<AdminSystemSettingDto?> UpdateAdminSettingAsync(int id, UpdateAdminSettingRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PublicSettingDto>> GetPublicSettingsAsync(CancellationToken ct = default);

    Task<IReadOnlyList<AdminDisplayMessageDto>> GetDisplayMessagesAsync(DisplayMessageListQuery query, CancellationToken ct = default);
    Task<AdminDisplayMessageDto> CreateDisplayMessageAsync(UpsertDisplayMessageRequest request, CancellationToken ct = default);
    Task<AdminDisplayMessageDto?> UpdateDisplayMessageAsync(int id, UpsertDisplayMessageRequest request, CancellationToken ct = default);
    Task<AdminDisplayMessageDto?> PatchDisplayMessageStatusAsync(int id, PatchStatusRequest request, CancellationToken ct = default);
    Task<bool> DeleteDisplayMessageAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<PublicDisplayMessageDto>> GetPublicDisplayMessagesAsync(string languageCode, CancellationToken ct = default);

    Task<IReadOnlyList<AdminVoiceTemplateDto>> GetVoiceTemplatesAsync(VoiceTemplateListQuery query, CancellationToken ct = default);
    Task<AdminVoiceTemplateDto> CreateVoiceTemplateAsync(UpsertVoiceTemplateRequest request, CancellationToken ct = default);
    Task<AdminVoiceTemplateDto?> UpdateVoiceTemplateAsync(int id, UpsertVoiceTemplateRequest request, CancellationToken ct = default);
    Task<AdminVoiceTemplateDto?> PatchVoiceTemplateStatusAsync(int id, PatchStatusRequest request, CancellationToken ct = default);
    Task<bool> DeleteVoiceTemplateAsync(int id, CancellationToken ct = default);
    Task<VoiceTemplateDto?> GetRuntimeVoiceTemplateAsync(string languageCode, string eventType, CancellationToken ct = default);

    Task<IReadOnlyList<ServiceTranslationDto>> GetServiceTranslationsAsync(int serviceId, CancellationToken ct = default);
    Task SaveServiceTranslationsAsync(int serviceId, IReadOnlyList<UpsertServiceTranslationRequest> items, CancellationToken ct = default);
    Task<IReadOnlyList<SubServiceTranslationDto>> GetSubServiceTranslationsAsync(int subServiceId, CancellationToken ct = default);
    Task SaveSubServiceTranslationsAsync(int subServiceId, IReadOnlyList<UpsertSubServiceTranslationRequest> items, CancellationToken ct = default);
}
