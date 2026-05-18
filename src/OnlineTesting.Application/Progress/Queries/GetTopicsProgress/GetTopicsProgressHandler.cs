using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Progress.Queries.GetTopicsProgress;

public class GetTopicsProgressHandler : IRequestHandler<GetTopicsProgressQuery, List<TopicProgressDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILanguageContext _lang;

    public GetTopicsProgressHandler(IApplicationDbContext db, ICurrentUser currentUser, ILanguageContext lang)
    {
        _db = db;
        _currentUser = currentUser;
        _lang = lang;
    }

    public async Task<List<TopicProgressDto>> Handle(GetTopicsProgressQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var userAttemptIds = _db.Attempts.Where(a => a.UserId == userId && a.TestLinkId == null).Select(a => a.Id);

        var topicStats = await _db.AttemptQuestions
            .Where(aq => userAttemptIds.Contains(aq.AttemptId) && aq.ChosenAnswerId != null)
            .Join(_db.Questions, aq => aq.QuestionId, q => q.Id, (aq, q) => new { aq.IsCorrect, q.TopicId })
            .GroupBy(x => x.TopicId)
            .Select(g => new
            {
                TopicId = g.Key,
                Total = g.Count(),
                Correct = g.Count(x => x.IsCorrect == true)
            })
            .ToListAsync(ct);

        var allTopics = await _db.Topics
            .Include(t => t.Translations)
            .OrderBy(t => t.OrderIndex)
            .ToListAsync(ct);

        return allTopics.Select(topic =>
        {
            var stat = topicStats.FirstOrDefault(s => s.TopicId == topic.Id);
            var total = stat?.Total ?? 0;
            var correct = stat?.Correct ?? 0;
            var acc = total > 0 ? Math.Round((double)correct / total * 100, 1) : 0.0;
            return new TopicProgressDto(
                topic.Id,
                ProgressHelpers.GetTopicName(topic, _lang),
                total, correct, acc,
                ProgressHelpers.GetGrade(total, acc));
        }).ToList();
    }
}
