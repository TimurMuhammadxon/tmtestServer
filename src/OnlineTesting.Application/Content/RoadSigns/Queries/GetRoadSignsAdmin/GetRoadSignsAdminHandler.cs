using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Content.RoadSigns.Queries.GetRoadSignsAdmin;

public class GetRoadSignsAdminHandler : IRequestHandler<GetRoadSignsAdminQuery, List<AdminRoadSignCategoryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageService _storage;

    public GetRoadSignsAdminHandler(IApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<List<AdminRoadSignCategoryDto>> Handle(GetRoadSignsAdminQuery request, CancellationToken ct)
    {
        var categories = await _db.RoadSignCategories
            .AsNoTracking()
            .OrderBy(c => c.OrderIndex)
            .Select(c => new
            {
                c.Id,
                c.OrderIndex,
                Translations = c.Translations.Select(t => new { t.LanguageCode, t.Name }).ToList()
            })
            .ToListAsync(ct);

        var signs = await _db.RoadSigns
            .AsNoTracking()
            .OrderBy(s => s.OrderIndex)
            .Select(s => new
            {
                s.Id,
                s.CategoryId,
                s.Code,
                s.OrderIndex,
                s.IsActive,
                s.ImageKey,
                Translations = s.Translations.Select(t => new { t.LanguageCode, t.Name, t.Description }).ToList()
            })
            .ToListAsync(ct);

        var signsByCategory = signs
            .GroupBy(s => s.CategoryId)
            .ToDictionary(g => g.Key, g => g.Select(s => new AdminRoadSignDto(
                s.Id,
                s.CategoryId,
                s.Code,
                s.OrderIndex,
                s.IsActive,
                string.IsNullOrWhiteSpace(s.ImageKey) ? null : _storage.GetPublicUrl(s.ImageKey!),
                s.Translations.Select(t => new AdminSignTranslationDto(t.LanguageCode, t.Name, t.Description)).ToList()
            )).ToList());

        return categories.Select(c => new AdminRoadSignCategoryDto(
            c.Id,
            c.OrderIndex,
            c.Translations.Select(t => new AdminCategoryTranslationDto(t.LanguageCode, t.Name)).ToList(),
            signsByCategory.TryGetValue(c.Id, out var list) ? list : new List<AdminRoadSignDto>()
        )).ToList();
    }
}
