using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Teacher.Applications.Admin.Queries.GetApplicationsList;

public class GetApplicationsListHandler : IRequestHandler<GetApplicationsListQuery, PagedResult<ApplicationListItemDto>>
{
    private readonly IApplicationDbContext _db;

    public GetApplicationsListHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<ApplicationListItemDto>> Handle(GetApplicationsListQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        var query = _db.TeacherApplications.AsNoTracking().AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(a => a.Status == request.Status.Value);

        var total = await query.CountAsync(ct);

        var rows = await query
            .OrderByDescending(a => a.SubmittedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Join(_db.Users, a => a.UserId, u => u.Id, (a, u) => new
            {
                a.Id, a.UserId, UserEmail = u.Email,
                a.FullName, a.PhoneNumber, a.OrganizationName,
                a.Status, a.SubmittedAt, a.ReviewedAt
            })
            .ToListAsync(ct);

        var items = rows.Select(r => new ApplicationListItemDto(
            r.Id, r.UserId, r.UserEmail,
            r.FullName, r.PhoneNumber, r.OrganizationName,
            r.Status.ToString(), r.SubmittedAt, r.ReviewedAt))
            .ToList();

        return new PagedResult<ApplicationListItemDto>(items, page, size, total);
    }
}
