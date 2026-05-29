using Microsoft.EntityFrameworkCore;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;
using SmartQ.Domain.Enums;
using SmartQ.Infrastructure.Persistence;

namespace SmartQ.Infrastructure.Services;

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly SmartQDbContext _db;

    public ServiceCatalogService(SmartQDbContext db) => _db = db;

    public async Task<IReadOnlyList<ServiceDto>> GetServicesAsync(string languageCode, CancellationToken ct = default)
    {
        var langId = await GetLanguageIdAsync(languageCode, ct);
        var services = await _db.Services.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(ct);

        var translations = await _db.ServiceTranslations.AsNoTracking()
            .Where(t => t.LanguageId == langId)
            .ToDictionaryAsync(t => t.ServiceId, ct);

        return services.Select(s =>
        {
            var tr = translations.GetValueOrDefault(s.Id);
            return new ServiceDto(s.Id, s.Code, tr?.Name ?? s.Name, tr?.Description ?? s.Description, s.Icon, s.DisplayOrder);
        }).ToList();
    }

    public async Task<IReadOnlyList<SubServiceDto>> GetSubServicesAsync(int serviceId, string languageCode, CancellationToken ct = default)
    {
        var langId = await GetLanguageIdAsync(languageCode, ct);
        var subs = await _db.SubServices.AsNoTracking()
            .Where(s => s.ServiceId == serviceId && s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(ct);

        var translations = await _db.SubServiceTranslations.AsNoTracking()
            .Where(t => t.LanguageId == langId && subs.Select(s => s.Id).Contains(t.SubServiceId))
            .ToDictionaryAsync(t => t.SubServiceId, ct);

        var subIds = subs.Select(s => s.Id).ToList();
        var waitingCounts = await _db.Tokens.AsNoTracking()
            .Where(t => subIds.Contains(t.SubServiceId) && t.Status == TokenStatus.WAITING)
            .GroupBy(t => t.SubServiceId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

        return subs.Select(s =>
        {
            var tr = translations.GetValueOrDefault(s.Id);
            var waiting = waitingCounts.GetValueOrDefault(s.Id);
            return new SubServiceDto(
                s.Id, s.ServiceId, s.Code, tr?.Name ?? s.Name, tr?.Description ?? s.Description,
                s.TokenPrefix, s.Icon, s.EstimatedServiceMinutes, waiting,
                s.EstimatedServiceMinutes + waiting * 2);
        }).ToList();
    }

    public async Task<KioskStatusDto> GetKioskStatusAsync(CancellationToken ct = default)
    {
        var settings = await _db.SystemSettings.AsNoTracking()
            .Where(s => s.IsActive)
            .ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue, ct);

        var activeStaff = await _db.StaffUsers.AsNoTracking().CountAsync(s => s.IsActive, ct);
        var avgWait = await _db.Tokens.AsNoTracking()
            .Where(t => t.Status == TokenStatus.WAITING)
            .Select(t => (int?)t.EstimatedWaitMinutes)
            .AverageAsync(ct) ?? 8;

        return new KioskStatusDto(
            settings.GetValueOrDefault("BRANCH_ID", "BR-9904"),
            settings.GetValueOrDefault("BRANCH_NAME", "SmartQ Bank"),
            settings.GetValueOrDefault("KIOSK_VERSION", "v2.4"),
            true, activeStaff, (int)avgWait);
    }

    private async Task<int> GetLanguageIdAsync(string languageCode, CancellationToken ct)
    {
        var lang = await _db.Languages.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Code == languageCode && l.IsActive, ct);
        if (lang != null) return lang.Id;
        var def = await _db.Languages.AsNoTracking().FirstAsync(l => l.IsDefault, ct);
        return def.Id;
    }
}
