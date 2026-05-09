using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.SetQuestionActive;

public record SetQuestionActiveCommand(Guid Id, bool IsActive) : IRequest;