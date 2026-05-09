using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.SetTopicActive;

public record SetTopicActiveCommand(Guid Id, bool IsActive) : IRequest;