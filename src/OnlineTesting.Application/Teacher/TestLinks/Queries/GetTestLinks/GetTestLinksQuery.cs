using MediatR;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Teacher.TestLinks.Queries.GetTestLinks;

public record GetTestLinksQuery(int Page, int PageSize) : IRequest<PagedResult<TestLinkListItemDto>>;

public record TestLinkListItemDto(
    Guid Id,
    string Title,
    string Code,
    string FlowType,
    Guid? GroupId,
    int MaxAttempts,
    DateTime ExpiresAt,
    bool IsActive,
    DateTime CreatedAt,
    int AttemptCount
);
