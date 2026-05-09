using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.UpdateTopic;

public record UpdateTopicCommand(
    Guid Id,
    string Code,
    int OrderIndex,
    bool IsDemo) : IRequest;