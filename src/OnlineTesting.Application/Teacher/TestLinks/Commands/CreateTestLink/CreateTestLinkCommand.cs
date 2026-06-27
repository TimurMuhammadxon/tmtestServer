using MediatR;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Teacher.TestLinks.Commands.CreateTestLink;

public record CreateTestLinkCommand(
    string Title,
    FlowType FlowType,
    Guid? BiletId,
    List<Guid>? TopicIds,
    int? QuestionCount,
    Guid? GroupId,
    int MaxAttempts,
    DateTime ExpiresAt,
    bool ShowExplanations = false
) : IRequest<TestLinkDto>;

public record TestLinkDto(
    Guid Id,
    string Title,
    string Code,
    string FlowType,
    Guid? BiletId,
    List<Guid> TopicIds,
    int? QuestionCount,
    Guid? GroupId,
    int MaxAttempts,
    DateTime ExpiresAt,
    bool IsActive,
    bool ShowExplanations,
    DateTime CreatedAt
);
