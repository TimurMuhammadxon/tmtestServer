using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Content.RoadSigns.Queries.GetRoadSigns;

public class GetRoadSignsHandler : IRequestHandler<GetRoadSignsQuery, List<RoadSignCategoryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ILanguageContext _lang;
    private readonly IStorageService _storage;

    public GetRoadSignsHandler(IApplicationDbContext db, ILanguageContext lang, IStorageService storage)
    {
        _db = db;
        _lang = lang;
        _storage = storage;
    }

    public async Task<List<RoadSignCategoryDto>> Handle(GetRoadSignsQuery request, CancellationToken ct)
    {
        var lang = _lang.RequestedLanguage;
        var fallback = _lang.DefaultLanguage;

        static string Pick(IEnumerable<(string Code, string Text)> tr, string lang, string fallback)
        {
            var list = tr.ToList();
            return list.FirstOrDefault(t => t.Code == lang).Text
                ?? list.FirstOrDefault(t => t.Code == fallback).Text
                ?? "(no translation)";
        }

        var categories = await _db.RoadSignCategories
            .AsNoTracking()
            .OrderBy(c => c.OrderIndex)
            .Select(c => new
            {
                c.Id,
                c.OrderIndex,
                Translations = c.Translations
                    .Where(t => t.LanguageCode == lang || t.LanguageCode == fallback)
                    .Select(t => new { t.LanguageCode, t.Name })
                    .ToList()
            })
            .ToListAsync(ct);

        var signs = await _db.RoadSigns
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.OrderIndex)
            .Select(s => new
            {
                s.Id,
                s.CategoryId,
                s.Code,
                s.ImageKey,
                Translations = s.Translations
                    .Where(t => t.LanguageCode == lang || t.LanguageCode == fallback)
                    .Select(t => new { t.LanguageCode, t.Name, t.Description })
                    .ToList()
            })
            .ToListAsync(ct);

        var signsByCategory = signs
            .GroupBy(s => s.CategoryId)
            .ToDictionary(g => g.Key, g => g.Select(s =>
            {
                var name = Pick(s.Translations.Select(t => (t.LanguageCode, t.Name)), lang, fallback);
                var descList = s.Translations.ToList();
                var description = descList.FirstOrDefault(t => t.LanguageCode == lang)?.Description
                    ?? descList.FirstOrDefault(t => t.LanguageCode == fallback)?.Description;
                var imageUrl = string.IsNullOrWhiteSpace(s.ImageKey) ? null : _storage.GetPublicUrl(s.ImageKey!);
                return new RoadSignDto(s.Id, s.Code, name, description, imageUrl);
            }).ToList());

        return categories
            .Select(c => new RoadSignCategoryDto(
                c.Id,
                c.OrderIndex,
                Pick(c.Translations.Select(t => (t.LanguageCode, t.Name)), lang, fallback),
                signsByCategory.TryGetValue(c.Id, out var list) ? list : new List<RoadSignDto>()))
            // hide empty categories from the public view
            .Where(c => c.Signs.Count > 0)
            .ToList();
    }
}
