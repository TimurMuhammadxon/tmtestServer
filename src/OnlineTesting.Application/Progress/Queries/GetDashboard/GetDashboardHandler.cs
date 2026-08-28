using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Constants;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Progress.Queries.GetDashboard;

public class GetDashboardHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILanguageContext _lang;

    public GetDashboardHandler(IApplicationDbContext db, ICurrentUser currentUser, ILanguageContext lang)
    {
        _db = db;
        _currentUser = currentUser;
        _lang = lang;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var allDates = await _db.UserDailyActivities
            .Where(a => a.UserId == userId)
            .Select(a => a.ActivityDate)
            .ToListAsync(ct);

        var dateSet = allDates.ToHashSet();
        var today = Common.AppTime.Today;
        var currentStreak = ComputeCurrentStreak(dateSet, today);
        var longestStreak = ComputeLongestStreak(allDates);

        var userAttemptIds = _db.Attempts.Where(a => a.UserId == userId && a.TestLinkId == null).Select(a => a.Id);

        // Weekly activity — last 7 local (UTC+5) days, computed server-side from answers.
        var weekSinceUtc = Common.AppTime.StartOfDayUtc(today.AddDays(-6));
        var weekAnswers = await _db.AttemptQuestions
            .Where(aq => userAttemptIds.Contains(aq.AttemptId) && aq.ChosenAnswerId != null && aq.AnsweredAt >= weekSinceUtc)
            .Select(aq => new { aq.AnsweredAt, aq.IsCorrect })
            .ToListAsync(ct);

        var byDay = weekAnswers
            .Where(a => a.AnsweredAt != null)
            .GroupBy(a => Common.AppTime.LocalDate(a.AnsweredAt!.Value))
            .ToDictionary(g => g.Key, g => new { Total = g.Count(), Correct = g.Count(x => x.IsCorrect == true) });

        var weeklyActivity = new List<DailyActivityDto>();
        for (var i = 6; i >= 0; i--)
        {
            var d = today.AddDays(-i);
            byDay.TryGetValue(d, out var s);
            var total = s?.Total ?? 0;
            var acc = total > 0 ? Math.Round((double)s!.Correct / total * 100, 1) : 0.0;
            weeklyActivity.Add(new DailyActivityDto(d, total, acc));
        }

        var answerStats = await _db.AttemptQuestions
            .Where(aq => userAttemptIds.Contains(aq.AttemptId) && aq.ChosenAnswerId != null)
            .GroupBy(_ => true)
            .Select(g => new { Total = g.Count(), Correct = g.Count(x => x.IsCorrect == true) })
            .FirstOrDefaultAsync(ct);

        var totalAnswered = answerStats?.Total ?? 0;
        var totalCorrect = answerStats?.Correct ?? 0;

        var accuracy = totalAnswered > 0
            ? Math.Round((double)totalCorrect / totalAnswered * 100, 1)
            : 0.0;

        // Mastery / coverage — by UNIQUE active questions (not inflated by repeats).
        // Denominator is dynamic so it grows as new questions are added.
        var activeQuestions = _db.Questions.Where(q => q.IsActive).Select(q => q.Id);
        var totalQuestions = await activeQuestions.CountAsync(ct);

        var coveredQuestions = await _db.AttemptQuestions
            .Where(aq => userAttemptIds.Contains(aq.AttemptId) && aq.ChosenAnswerId != null)
            .Join(activeQuestions, aq => aq.QuestionId, id => id, (aq, id) => aq.QuestionId)
            .Distinct()
            .CountAsync(ct);

        var masteredQuestions = await _db.AttemptQuestions
            .Where(aq => userAttemptIds.Contains(aq.AttemptId) && aq.IsCorrect == true)
            .Join(activeQuestions, aq => aq.QuestionId, id => id, (aq, id) => aq.QuestionId)
            .Distinct()
            .CountAsync(ct);

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

        var examResults = await _db.Attempts
            .Where(a => a.UserId == userId
                && a.TestLinkId == null
                && a.Flow == FlowType.Exam
                && a.Status != AttemptStatus.InProgress
                && a.CorrectCount != null)
            .OrderByDescending(a => a.FinishedAt)
            .Take(5)
            .Select(a => a.CorrectCount!.Value)
            .ToListAsync(ct);

        var totalTopics = await _db.Topics.CountAsync(ct);
        var goodTopics = topicStats.Count(t => t.Total >= 5 && (double)t.Correct / t.Total >= 0.65);
        var prediction = ComputePrediction(examResults, goodTopics, totalTopics, totalAnswered);

        var weakTopicStats = topicStats
            .Where(t => t.Total >= 5 && (double)t.Correct / t.Total < 0.65)
            .OrderBy(t => (double)t.Correct / t.Total)
            .Take(5)
            .ToList();

        var weakTopics = new List<WeakTopicDto>();
        if (weakTopicStats.Count > 0)
        {
            var weakIds = weakTopicStats.Select(t => t.TopicId).ToList();
            var topics = await _db.Topics
                .Include(t => t.Translations)
                .Where(t => weakIds.Contains(t.Id))
                .ToListAsync(ct);

            weakTopics = weakTopicStats
                .Select(s =>
                {
                    var topic = topics.FirstOrDefault(t => t.Id == s.TopicId);
                    if (topic is null) return null;
                    var acc = Math.Round((double)s.Correct / s.Total * 100, 1);
                    return new WeakTopicDto(
                        topic.Id,
                        ProgressHelpers.GetTopicName(topic, _lang),
                        s.Total, s.Correct, acc,
                        ProgressHelpers.GetGrade(s.Total, acc, _lang.RequestedLanguage));
                })
                .Where(x => x is not null)
                .ToList()!;
        }

        var recentRaw = await _db.Attempts
            .Where(a => a.UserId == userId && a.TestLinkId == null && a.Status != AttemptStatus.InProgress)
            .OrderByDescending(a => a.FinishedAt)
            .Take(5)
            .Select(a => new
            {
                a.Id, a.Flow, a.Status, a.CorrectCount, a.StartedAt, a.FinishedAt,
                TotalQuestions = _db.AttemptQuestions.Count(aq => aq.AttemptId == a.Id)
            })
            .ToListAsync(ct);

        var recentAttempts = recentRaw
            .Select(a => new RecentAttemptDto(
                a.Id, a.Flow.ToString(), a.Status.ToString(),
                a.CorrectCount, a.TotalQuestions, a.StartedAt, a.FinishedAt))
            .ToList();

        return new DashboardDto(
            currentStreak, longestStreak,
            GetLevel(totalCorrect, _lang.RequestedLanguage),
            totalCorrect, totalAnswered, accuracy,
            prediction, weakTopics, recentAttempts, weeklyActivity,
            totalQuestions, coveredQuestions, masteredQuestions);
    }

    private static int ComputeCurrentStreak(HashSet<DateOnly> dates, DateOnly today)
    {
        int streak = 0;
        var check = dates.Contains(today) ? today : today.AddDays(-1);
        while (dates.Contains(check))
        {
            streak++;
            check = check.AddDays(-1);
        }
        return streak;
    }

    private static int ComputeLongestStreak(List<DateOnly> dates)
    {
        if (dates.Count == 0) return 0;
        var sorted = dates.Distinct().OrderBy(d => d).ToList();
        int longest = 1, current = 1;
        for (var i = 1; i < sorted.Count; i++)
        {
            if (sorted[i] == sorted[i - 1].AddDays(1))
                current++;
            else
                current = 1;
            if (current > longest) longest = current;
        }
        return longest;
    }

    private static int ComputePrediction(List<int> examResults, int goodTopics, int totalTopics, int totalAnswered)
    {
        double score = 0;
        if (examResults.Count > 0)
            score += examResults.Average(c => (double)c / Attempt.ExamQuestionsCount) * 60;

        if (totalTopics > 0)
            score += (double)goodTopics / totalTopics * 25;

        score += Math.Min(totalAnswered / 500.0, 1.0) * 15;

        return (int)Math.Min(score, 95);
    }

    private static string GetLevel(int totalCorrect, string lang)
    {
        var tier = totalCorrect switch
        {
            < 50 => 0, < 150 => 1, < 300 => 2, < 500 => 3, < 1000 => 4, _ => 5
        };
        return lang switch
        {
            Languages.Ru => new[] { "Новичок", "Начинающий", "Практикант", "Уверенный", "Опытный", "Мастер" }[tier],
            Languages.UzCyrl => new[] { "Бошловчи", "Ўрганувчи", "Амалиётчи", "Ишончли", "Тажрибали", "Уста" }[tier],
            _ => new[] { "Boshlovchi", "O'rganuvchi", "Amaliyotchi", "Ishonchli", "Tajribali", "Usta" }[tier],
        };
    }

}
