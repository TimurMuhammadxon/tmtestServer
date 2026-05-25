using MediatR;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Users.Admin.Queries.GetUsersList;

public record GetUsersListQuery(string? Search, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<UserAdminDto>>;

public record UserAdminDto(
    Guid Id,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? SubscriptionExpiresAt);
