using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Constants;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Tests.Admin.Questions.Queries.GetQuestionsList;

public class GetQuestionsListHandler : IRequestHandler<GetQuestionsListQuery, PagedResult<QuestionAdminListItemDto>>
{
    private readonly IApplicationDbContext _db;
    public GetQuestionsListHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<QuestionAdminListItemDto>> Handle(GetQuestionsListQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        var query = _db.Questions.AsNoTracking().AsQueryable();

        if (request.TopicId.HasValue)
            query = query.Where(q => q.TopicId == request.TopicId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim().ToLower()}%";
            query = query.Where(q =>
                _db.QuestionTranslations.Any(qt =>
                    qt.QuestionId == q.Id && EF.Functions.Like(qt.Text.ToLower(), pattern)));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(q => q.UpdatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(q => new QuestionAdminListItemDto(
                q.Id,
                q.TopicId,
                q.ImageKey,
                q.IsActive,
                q.Translations
                    .OrderBy(t => t.LanguageCode == Languages.Default ? 0 : 1)
                    .Select(t => t.Text)
                    .FirstOrDefault() ?? "(no translation)",
                q.Answers.Count))
            .ToListAsync(ct);

        return new PagedResult<QuestionAdminListItemDto>(items, page, size, total);
    }
}