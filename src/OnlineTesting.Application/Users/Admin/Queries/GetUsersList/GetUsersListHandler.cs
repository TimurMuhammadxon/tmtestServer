using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Users.Admin.Queries.GetUsersList;

public class GetUsersListHandler : IRequestHandler<GetUsersListQuery, PagedResult<UserAdminDto>>
{
    private readonly IApplicationDbContext _db;
    public GetUsersListHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<UserAdminDto>> Handle(GetUsersListQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        var query = _db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim().ToLower()}%";
            query = query.Where(u => EF.Functions.Like(u.Email.ToLower(), pattern));
        }

        var total = await query.CountAsync(ct);

        var rows = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.Role,
                u.IsActive,
                u.CreatedAt,
                SubscriptionExpiresAt = _db.Subscriptions
                    .Where(s => s.UserId == u.Id)
                    .Select(s => (DateTime?)s.ExpiresAt)
                    .FirstOrDefault(),
            })
            .ToListAsync(ct);

        var items = rows
            .Select(r => new UserAdminDto(
                r.Id,
                r.Email,
                r.Role.ToString(),
                r.IsActive,
                r.CreatedAt,
                r.SubscriptionExpiresAt))
            .ToList();

        return new PagedResult<UserAdminDto>(items, page, size, total);
    }
}
