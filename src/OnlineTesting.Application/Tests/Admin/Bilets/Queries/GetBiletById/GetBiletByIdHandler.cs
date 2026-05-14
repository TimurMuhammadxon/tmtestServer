using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Queries.GetBiletById;

public class GetBiletByIdHandler : IRequestHandler<GetBiletByIdQuery, BiletDetailsDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ILanguageContext _lang;

    public GetBiletByIdHandler(IApplicationDbContext db, ILanguageContext lang)
    {
        _db = db;
        _lang = lang;
    }

    public async Task<BiletDetailsDto> Handle(GetBiletByIdQuery request, CancellationToken ct)
    {
        var requested = _lang.RequestedLanguage;
        var fallback = _lang.DefaultLanguage;

        var bilet = await _db.Bilets
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new NotFoundException($"Bilet '{request.Id}' not found.");

        var biletQuestions = await _db.BiletQuestions
            .AsNoTracking()
            .Where(bq => bq.BiletId == request.Id)
            .Include(bq => bq.Question!)
                .ThenInclude(q => q.Translations)
            .OrderBy(bq => bq.OrderIndex)
            .ToListAsync(ct);

        var questionIds = biletQuestions.Select(bq => bq.QuestionId).ToList();

        var answers = await _db.Answers
            .AsNoTracking()
            .Where(a => questionIds.Contains(a.QuestionId))
            .Include(a => a.Translations)
            .ToListAsync(ct);

        var answersByQuestion = answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.OrderBy(a => a.OrderIndex).ToList());

        var questions = biletQuestions
            .Select(bq =>
            {
                var q = bq.Question!;

                var (qText, qExplanation, qLang, qFallback) = ResolveQuestionTexts(
                    q.Translations, requested, fallback);

                var qAnswers = answersByQuestion.TryGetValue(q.Id, out var list)
                    ? list
                    : new List<Answer>();

                var answerDtos = qAnswers
                    .Select(a =>
                    {
                        var (aText, aLang, aFallback) = ResolveAnswerText(
                            a.Translations, requested, fallback);

                        return new BiletAnswerDto(a.Id, a.OrderIndex, aText, aLang, aFallback, a.IsCorrect);
                    })
                    .ToList();

                return new BiletQuestionDto(
                    bq.OrderIndex,
                    q.Id,
                    q.ImageKey,
                    qText,
                    qExplanation,
                    qLang,
                    qFallback,
                    answerDtos);
            })
            .ToList();

        return new BiletDetailsDto(
            bilet.Id,
            bilet.Number,
            bilet.IsDemo,
            bilet.IsActive,
            bilet.CreatedAt,
            bilet.UpdatedAt,
            questions);
    }

    private static (string Text, string? Explanation, string Language, bool IsFallback) ResolveQuestionTexts(
        IEnumerable<QuestionTranslation> translations, string requested, string fallback)
    {
        var list = translations.ToList();

        var exact = list.FirstOrDefault(t => t.LanguageCode == requested);
        if (exact is not null)
            return (exact.Text, exact.Explanation, exact.LanguageCode, false);

        var def = list.FirstOrDefault(t => t.LanguageCode == fallback);
        if (def is not null)
            return (def.Text, def.Explanation, def.LanguageCode, true);

        return ("(no translation)", null, fallback, true);
    }

    private static (string Text, string Language, bool IsFallback) ResolveAnswerText(
        IEnumerable<AnswerTranslation> translations, string requested, string fallback)
    {
        var list = translations.ToList();

        var exact = list.FirstOrDefault(t => t.LanguageCode == requested);
        if (exact is not null)
            return (exact.Text, exact.LanguageCode, false);

        var def = list.FirstOrDefault(t => t.LanguageCode == fallback);
        if (def is not null)
            return (def.Text, def.LanguageCode, true);

        return ("(no translation)", fallback, true);
    }
}