using SmartQ.Application.DTOs;

namespace SmartQ.Application.Interfaces;

public interface IServiceCatalogService
{
    Task<IReadOnlyList<ServiceDto>> GetServicesAsync(string languageCode, CancellationToken ct = default);
    Task<IReadOnlyList<SubServiceDto>> GetSubServicesAsync(int serviceId, string languageCode, CancellationToken ct = default);
    Task<KioskStatusDto> GetKioskStatusAsync(CancellationToken ct = default);
}
