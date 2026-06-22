using MediatR;

namespace OnlineTesting.Application.Progress.Queries.GetErrorQuestionDetail;

public record GetErrorQuestionDetailQuery(Guid QuestionId) : IRequest<ErrorQuestionDetailDto>;

public record ErrorQuestionDetailDto(
    Guid QuestionId,
    string QuestionText,
    string? ImageUrl,
    string? Explanation,
    string TopicName,
    List<ErrorAnswerDto> Answers,
    Guid? LastChosenAnswerId);

public record ErrorAnswerDto(
    Guid Id,
    string Text,
    bool IsCorrect);
