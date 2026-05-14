using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Queries.GetBilets;

public class GetBiletsHandler : IRequestHandler<GetBiletsQuery, PagedResult<BiletListItemDto>>
{
    private const int MaxPageSize = 200;
    private const int DefaultPageSize = 20;

    private readonly IApplicationDbContext _db;
    public GetBiletsHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<BiletListItemDto>> Handle(GetBiletsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > MaxPageSize ? DefaultPageSize : request.PageSize;

        var query = _db.Bilets.AsNoTracking();

        if (request.IsActive.HasValue)
            query = query.Where(b => b.IsActive == request.IsActive.Value);
        if (request.IsDemo.HasValue)
            query = query.Where(b => b.IsDemo == request.IsDemo.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(b => b.Number)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BiletListItemDto(
                b.Id,
                b.Number,
                b.IsDemo,
                b.IsActive,
                b.BiletQuestions.Count,
                b.CreatedAt,
                b.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<BiletListItemDto>(items, total, page, pageSize);
    }
}