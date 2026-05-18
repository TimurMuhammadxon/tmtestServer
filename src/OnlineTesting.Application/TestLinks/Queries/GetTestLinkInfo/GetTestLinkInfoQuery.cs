using MediatR;

namespace OnlineTesting.Application.TestLinks.Queries.GetTestLinkInfo;

public record GetTestLinkInfoQuery(string Code) : IRequest<TestLinkInfoDto>;

public record TestLinkInfoDto(
    string Title,
    string FlowType,
    int MaxAttempts,
    DateTime ExpiresAt,
    int AttemptsUsed
);
