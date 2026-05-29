using Microsoft.EntityFrameworkCore;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;
using SmartQ.Infrastructure.Persistence;

namespace SmartQ.Infrastructure.Services;

public class LanguageService : ILanguageService
{
    private readonly SmartQDbContext _db;

    public LanguageService(SmartQDbContext db) => _db = db;

    public async Task<IReadOnlyList<LanguageDto>> GetActiveLanguagesAsync(CancellationToken ct = default)
    {
        return await _db.Languages.AsNoTracking()
            .Where(l => l.IsActive)
            .OrderBy(l => l.DisplayOrder)
            .Select(l => new LanguageDto(l.Id, l.Code, l.Name, l.NativeName, l.IsDefault))
            .ToListAsync(ct);
    }
}
