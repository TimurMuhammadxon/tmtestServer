using MediatR;

namespace OnlineTesting.Application.Progress.Queries.GetDashboard;

public record GetDashboardQuery : IRequest<DashboardDto>;

public record DashboardDto(
    int CurrentStreak,
    int LongestStreak,
    string Level,
    int TotalCorrect,
    int TotalAnswered,
    double AccuracyPercent,
    int ExamPassPrediction,
    List<WeakTopicDto> WeakTopics,
    List<RecentAttemptDto> RecentAttempts,
    List<DailyActivityDto> WeeklyActivity,
    int TotalQuestions,
    int CoveredQuestions,
    int MasteredQuestions);

public record DailyActivityDto(
    DateOnly Date,
    int AnswersCount,
    double AccuracyPercent);

public record WeakTopicDto(
    Guid TopicId,
    string TopicName,
    int TotalAnswered,
    int CorrectCount,
    double AccuracyPercent,
    string Grade);

public record RecentAttemptDto(
    Guid Id,
    string Flow,
    string Status,
    int? CorrectCount,
    int TotalQuestions,
    DateTime StartedAt,
    DateTime? FinishedAt);
