using MediatR;

namespace OnlineTesting.Application.Progress.Queries.GetTopicsProgress;

public record GetTopicsProgressQuery : IRequest<List<TopicProgressDto>>;

public record TopicProgressDto(
    Guid TopicId,
    string TopicName,
    int TotalAnswered,
    int CorrectCount,
    double AccuracyPercent,
    string Grade);
