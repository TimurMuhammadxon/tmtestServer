using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Tests.Solutions.Bilets.Queries;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Tests.Solutions.Bilets.Queries.GetPublicBiletById;

public class GetPublicBiletByIdHandler : IRequestHandler<GetPublicBiletByIdQuery, PublicBiletDetailsDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILanguageContext _lang;

    public GetPublicBiletByIdHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        ILanguageContext lang)
    {
        _db = db;
        _currentUser = currentUser;
        _lang = lang;
    }

    public async Task<PublicBiletDetailsDto> Handle(GetPublicBiletByIdQuery request, CancellationToken ct)
    {
        var requested = _lang.RequestedLanguage;
        var fallback = _lang.DefaultLanguage;

        var bilet = await _db.Bilets
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Id && b.IsActive, ct)
            ?? throw new NotFoundException($"Bilet '{request.Id}' not found.");

        if (!_currentUser.IsAuthenticated && !bilet.IsDemo)
            throw new UnauthorizedException("Authentication required to access this bilet.");

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

                        return new PublicBiletAnswerDto(a.Id, a.OrderIndex, aText, aLang, aFallback, a.IsCorrect);
                    })
                    .ToList();

                return new PublicBiletQuestionDto(
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

        return new PublicBiletDetailsDto(
            bilet.Id,
            bilet.Number,
            bilet.IsDemo,
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