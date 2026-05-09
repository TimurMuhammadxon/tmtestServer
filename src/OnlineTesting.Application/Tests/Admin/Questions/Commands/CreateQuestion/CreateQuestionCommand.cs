using MediatR;
using OnlineTesting.Application.Tests.Common;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.CreateQuestion;

public record CreateQuestionCommand(
    Guid TopicId,
    string? ImageKey,
    List<QuestionTranslationDto> Translations,
    List<AnswerInputDto> Answers) : IRequest<CreateQuestionResponse>;

public record CreateQuestionResponse(Guid Id);