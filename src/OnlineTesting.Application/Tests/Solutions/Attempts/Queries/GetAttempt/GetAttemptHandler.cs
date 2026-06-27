using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Tests.Solutions.Attempts.Queries.GetAttempt;

public class GetAttemptHandler : IRequestHandler<GetAttemptQuery, AttemptDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILanguageContext _lang;
    private readonly IStorageService _storage;

    public GetAttemptHandler(IApplicationDbContext db, ICurrentUser currentUser, ILanguageContext lang, IStorageService storage)
    {
        _db = db;
        _currentUser = currentUser;
        _lang = lang;
        _storage = storage;
    }

    public async Task<AttemptDto> Handle(GetAttemptQuery request, CancellationToken ct)
    {
        var attempt = await _db.Attempts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, ct)
            ?? throw new NotFoundException($"Attempt '{request.AttemptId}' not found.");

        if (attempt.UserId != _currentUser.UserId)
            throw new NotFoundException($"Attempt '{request.AttemptId}' not found.");

        var showExplanations = await DetermineShowExplanations(attempt, ct);

        var attemptQuestions = await _db.AttemptQuestions
            .AsNoTracking()
            .Where(aq => aq.AttemptId == request.AttemptId)
            .OrderBy(aq => aq.OrderIndex)
            .ToListAsync(ct);

        var questionIds = attemptQuestions.Select(aq => aq.QuestionId).ToList();

        var questions = await _db.Questions
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .Include(q => q.Translations)
            .ToDictionaryAsync(q => q.Id, ct);

        var answers = await _db.Answers
            .AsNoTracking()
            .Where(a => questionIds.Contains(a.QuestionId))
            .Include(a => a.Translations)
            .ToListAsync(ct);

        var answersByQuestion = answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.OrderBy(a => a.OrderIndex).ToList());

        var requested = _lang.RequestedLanguage;
        var fallback = _lang.DefaultLanguage;

        var questionDtos = attemptQuestions.Select(aq =>
        {
            var q = questions[aq.QuestionId];
            var (qText, qLang, qFallback) = ResolveText(
                q.Translations.Select(t => (t.LanguageCode, t.Text)), requested, fallback);

            string? explanation = null;
            if (showExplanations)
            {
                explanation = q.Translations
                    .FirstOrDefault(t => t.LanguageCode == requested)?.Explanation
                    ?? q.Translations.FirstOrDefault(t => t.LanguageCode == fallback)?.Explanation;
            }

            var qAnswers = answersByQuestion.TryGetValue(q.Id, out var list)
                ? list : new();

            var answerDtos = qAnswers.Select(a =>
            {
                var (aText, aLang, aFallback) = ResolveText(
                    a.Translations.Select(t => (t.LanguageCode, t.Text)), requested, fallback);
                return new AttemptAnswerDto(a.Id, a.OrderIndex, aText, aLang, aFallback, a.IsCorrect);
            }).ToList();

            var imageUrl = q.ImageKey is not null ? _storage.GetPublicUrl(q.ImageKey) : null;

            return new AttemptQuestionDto(
                aq.OrderIndex,
                aq.QuestionId,
                imageUrl,
                qText,
                qLang,
                qFallback,
                aq.ChosenAnswerId,
                aq.IsCorrect,
                aq.AnsweredAt,
                explanation,
                answerDtos);
        }).ToList();

        int? remainingSeconds = null;
        if (attempt.Flow == FlowType.Exam && attempt.Status == AttemptStatus.InProgress)
        {
            var elapsed = (DateTime.UtcNow - attempt.StartedAt).TotalSeconds;
            remainingSeconds = Math.Max(0, Attempt.ExamTimeLimitSeconds - (int)elapsed);
        }

        return new AttemptDto(
            attempt.Id,
            attempt.Flow.ToString(),
            attempt.Status.ToString(),
            attempt.StartedAt,
            attempt.FinishedAt,
            attempt.CorrectCount,
            questionDtos.Count,
            remainingSeconds,
            showExplanations,
            questionDtos);
    }

    private async Task<bool> DetermineShowExplanations(Attempt attempt, CancellationToken ct)
    {
        if (attempt.Flow == FlowType.Exam)
            return false;

        if (attempt.TestLinkId is not null)
        {
            var link = await _db.TestLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == attempt.TestLinkId.Value, ct);
            return link?.ShowExplanations ?? false;
        }

        return true;
    }

    private static (string Text, string Language, bool IsFallback) ResolveText(
        IEnumerable<(string Code, string Text)> translations, string requested, string fallback)
    {
        var list = translations.ToList();
        var exact = list.FirstOrDefault(t => t.Code == requested);
        if (exact != default) return (exact.Text, exact.Code, false);
        var def = list.FirstOrDefault(t => t.Code == fallback);
        if (def != default) return (def.Text, def.Code, true);
        return ("(no translation)", fallback, true);
    }
}
