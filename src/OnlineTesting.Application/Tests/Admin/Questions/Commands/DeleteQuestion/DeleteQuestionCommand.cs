using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.DeleteQuestion;

public record DeleteQuestionCommand(Guid Id) : IRequest;