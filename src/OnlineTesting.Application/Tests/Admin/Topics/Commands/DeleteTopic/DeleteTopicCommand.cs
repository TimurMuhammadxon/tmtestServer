using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.DeleteTopic;

public record DeleteTopicCommand(Guid Id) : IRequest;