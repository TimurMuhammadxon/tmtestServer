using MediatR;
using OnlineTesting.Application.Tests.Common;

namespace OnlineTesting.Application.Tests.Admin.Questions.Queries.GetQuestionById;

public record GetQuestionByIdQuery(Guid Id) : IRequest<QuestionAdminDto>;

public record QuestionAdminDto(
    Guid Id,
    Guid TopicId,
    string? ImageKey,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<QuestionTranslationDto> Translations,
    List<AnswerAdminDto> Answers);

public record AnswerAdminDto(
    Guid Id,
    int OrderIndex,
    bool IsCorrect,
    List<AnswerTranslationDto> Translations);