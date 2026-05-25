using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Constants;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Tests.Admin.Questions.Queries.GetQuestionsList;

public class GetQuestionsListHandler : IRequestHandler<GetQuestionsListQuery, PagedResult<QuestionAdminListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageService _storage;

    public GetQuestionsListHandler(IApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

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

        var rows = await query
            .OrderByDescending(q => q.UpdatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(q => new
            {
                q.Id,
                q.TopicId,
                q.ImageKey,
                q.IsActive,
                DefaultText = q.Translations
                    .OrderBy(t => t.LanguageCode == Languages.Default ? 0 : 1)
                    .Select(t => t.Text)
                    .FirstOrDefault() ?? "(no translation)",
                Answers = q.Answers
                    .OrderBy(a => a.OrderIndex)
                    .Select(a => new
                    {
                        a.Id,
                        a.OrderIndex,
                        a.IsCorrect,
                        Text = a.Translations
                            .OrderBy(t => t.LanguageCode == Languages.Default ? 0 : 1)
                            .Select(t => t.Text)
                            .FirstOrDefault() ?? "",
                    })
                    .ToList(),
            })
            .ToListAsync(ct);

        var items = rows
            .Select(r => new QuestionAdminListItemDto(
                r.Id,
                r.TopicId,
                r.ImageKey,
                r.ImageKey is not null ? _storage.GetPublicUrl(r.ImageKey) : null,
                r.IsActive,
                r.DefaultText,
                r.Answers.Select(a => new AnswerListItemDto(a.Id, a.OrderIndex, a.IsCorrect, a.Text)).ToList()))
            .ToList();

        return new PagedResult<QuestionAdminListItemDto>(items, page, size, total);
    }
}