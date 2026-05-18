using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Progress.Queries.GetErrorsAnalysis;

public class GetErrorsAnalysisHandler : IRequestHandler<GetErrorsAnalysisQuery, List<ErrorAnalysisItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILanguageContext _lang;

    public GetErrorsAnalysisHandler(IApplicationDbContext db, ICurrentUser currentUser, ILanguageContext lang)
    {
        _db = db;
        _currentUser = currentUser;
        _lang = lang;
    }

    public async Task<List<ErrorAnalysisItemDto>> Handle(GetErrorsAnalysisQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var userAttemptIds = _db.Attempts.Where(a => a.UserId == userId).Select(a => a.Id);

        var errorStats = await _db.AttemptQuestions
            .Where(aq => userAttemptIds.Contains(aq.AttemptId) && aq.ChosenAnswerId != null)
            .GroupBy(aq => aq.QuestionId)
            .Select(g => new
            {
                QuestionId = g.Key,
                Total = g.Count(),
                Errors = g.Count(x => x.IsCorrect == false)
            })
            .Where(x => x.Errors > 0)
            .OrderByDescending(x => x.Errors)
            .Take(20)
            .ToListAsync(ct);

        if (errorStats.Count == 0)
            return new List<ErrorAnalysisItemDto>();

        var questionIds = errorStats.Select(e => e.QuestionId).ToList();
        var questions = await _db.Questions
            .Include(q => q.Translations)
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync(ct);

        var topicIds = questions.Select(q => q.TopicId).Distinct().ToList();
        var topics = await _db.Topics
            .Include(t => t.Translations)
            .Where(t => topicIds.Contains(t.Id))
            .ToListAsync(ct);

        var reqLang = _lang.RequestedLanguage;
        var defLang = _lang.DefaultLanguage;

        return errorStats
            .Select(e =>
            {
                var q = questions.FirstOrDefault(x => x.Id == e.QuestionId);
                if (q is null) return null;
                var topic = topics.FirstOrDefault(t => t.Id == q.TopicId);
                if (topic is null) return null;

                var questionText = q.Translations.FirstOrDefault(t => t.LanguageCode == reqLang)?.Text
                    ?? q.Translations.FirstOrDefault(t => t.LanguageCode == defLang)?.Text
                    ?? "(no translation)";

                var errorRate = Math.Round((double)e.Errors / e.Total * 100, 1);

                return new ErrorAnalysisItemDto(
                    q.Id,
                    questionText,
                    topic.Id,
                    ProgressHelpers.GetTopicName(topic, _lang),
                    e.Errors,
                    e.Total,
                    errorRate);
            })
            .Where(x => x is not null)
            .ToList()!;
    }
}
