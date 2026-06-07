using MediatR;
using OnlineTesting.Application.Tests.Common;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.UpdateQuestion;

public record UpdateQuestionCommand(
    Guid Id,
    Guid TopicId,
    List<AnswerInputDto> Answers) : IRequest;