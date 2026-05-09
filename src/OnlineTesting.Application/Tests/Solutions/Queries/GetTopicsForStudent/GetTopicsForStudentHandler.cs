using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Solutions.Queries.GetTopicsForStudent;

public class GetTopicsForStudentHandler : IRequestHandler<GetTopicsForStudentQuery, List<TopicStudentDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ILanguageContext _lang;

    public GetTopicsForStudentHandler(IApplicationDbContext db, ILanguageContext lang)
    {
        _db = db;
        _lang = lang;
    }

    public async Task<List<TopicStudentDto>> Handle(GetTopicsForStudentQuery request, CancellationToken ct)
    {
        var lang = _lang.RequestedLanguage;
        var fallback = _lang.DefaultLanguage;

        var query = _db.Topics.AsNoTracking().Where(t => t.IsActive);
        if (request.GuestMode)
            query = query.Where(t => t.IsDemo);

        var raw = await query
            .OrderBy(t => t.OrderIndex)
            .Select(t => new
            {
                t.Id,
                t.Code,
                t.OrderIndex,
                t.IsDemo,
                Translations = t.Translations
                    .Where(tr => tr.LanguageCode == lang || tr.LanguageCode == fallback)
                    .Select(tr => new { tr.LanguageCode, tr.Name })
                    .ToList()
            })
            .ToListAsync(ct);

        return raw.Select(t =>
        {
            var preferred = t.Translations.FirstOrDefault(tr => tr.LanguageCode == lang);
            var actual = preferred ?? t.Translations.FirstOrDefault(tr => tr.LanguageCode == fallback);
            return new TopicStudentDto(
                t.Id,
                t.Code,
                t.OrderIndex,
                t.IsDemo,
                actual?.Name ?? "(no translation)",
                actual?.LanguageCode ?? fallback,
                preferred is null);
        }).ToList();
    }
}