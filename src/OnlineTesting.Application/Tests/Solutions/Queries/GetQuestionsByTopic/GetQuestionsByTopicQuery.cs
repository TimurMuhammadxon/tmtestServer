using MediatR;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Tests.Solutions.Queries.GetQuestionsByTopic;

public record GetQuestionsByTopicQuery(
    Guid TopicId,
    bool GuestMode,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<QuestionStudentDto>>;

public record QuestionStudentDto(
    Guid Id,
    string? ImageKey,
    string Text,
    string? Explanation,
    string Language,
    bool IsFallback,
    List<AnswerStudentDto> Answers);

public record AnswerStudentDto(
    Guid Id,
    int OrderIndex,
    bool IsCorrect,
    string Text,
    string Language,
    bool IsFallback);