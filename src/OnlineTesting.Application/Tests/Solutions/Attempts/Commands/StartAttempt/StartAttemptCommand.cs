using MediatR;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Tests.Solutions.Attempts.Commands.StartAttempt;

public record StartAttemptCommand(
    FlowType FlowType,
    Guid? BiletId,
    IReadOnlyList<Guid>? TopicIds,
    int? QuestionCount
) : IRequest<Guid>;
