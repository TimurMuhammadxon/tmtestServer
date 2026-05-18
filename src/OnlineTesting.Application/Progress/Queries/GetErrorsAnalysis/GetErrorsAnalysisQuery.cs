using MediatR;

namespace OnlineTesting.Application.Progress.Queries.GetErrorsAnalysis;

public record GetErrorsAnalysisQuery : IRequest<List<ErrorAnalysisItemDto>>;

public record ErrorAnalysisItemDto(
    Guid QuestionId,
    string QuestionText,
    Guid TopicId,
    string TopicName,
    int ErrorCount,
    int TotalAnswered,
    double ErrorRatePercent);
