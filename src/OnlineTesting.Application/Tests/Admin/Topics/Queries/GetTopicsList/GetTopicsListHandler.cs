using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Application.Tests.Common;

namespace OnlineTesting.Application.Tests.Admin.Topics.Queries.GetTopicsList;

public class GetTopicsListHandler : IRequestHandler<GetTopicsListQuery, PagedResult<TopicAdminDto>>
{
    private readonly IApplicationDbContext _db;
    public GetTopicsListHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<TopicAdminDto>> Handle(GetTopicsListQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        var query = _db.Topics.AsNoTracking();

        var total = await query.CountAsync(ct);

        var ordered = query
            .OrderBy(t => t.OrderIndex)
            .ThenBy(t => t.Code)
            .Skip((page - 1) * size)
            .Take(size);

        if (request.IncludeTranslations)
            ordered = ordered.Include(t => t.Translations);

        var topics = await ordered.ToListAsync(ct);

        var items = topics.Select(t => new TopicAdminDto(
            t.Id, t.Code, t.OrderIndex, t.IsDemo, t.IsActive,
            t.CreatedAt, t.UpdatedAt,
            request.IncludeTranslations
                ? t.Translations
                    .OrderBy(tr => tr.LanguageCode)
                    .Select(tr => new TopicTranslationDto(tr.LanguageCode, tr.Name))
                    .ToList()
                : null
        )).ToList();

        return new PagedResult<TopicAdminDto>(items, page, size, total);
    }
}