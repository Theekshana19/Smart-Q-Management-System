using SmartQ.Application.DTOs;

namespace SmartQ.Application.Interfaces;

public interface ILanguageService
{
    Task<IReadOnlyList<LanguageDto>> GetActiveLanguagesAsync(CancellationToken ct = default);
}
