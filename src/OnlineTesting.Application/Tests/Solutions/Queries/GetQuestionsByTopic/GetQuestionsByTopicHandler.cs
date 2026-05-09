using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Tests.Solutions.Queries.GetQuestionsByTopic;

public class GetQuestionsByTopicHandler : IRequestHandler<GetQuestionsByTopicQuery, PagedResult<QuestionStudentDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ILanguageContext _lang;

    public GetQuestionsByTopicHandler(IApplicationDbContext db, ILanguageContext lang)
    {
        _db = db;
        _lang = lang;
    }

    public async Task<PagedResult<QuestionStudentDto>> Handle(GetQuestionsByTopicQuery request, CancellationToken ct)
    {
        var topic = await _db.Topics.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TopicId, ct)
            ?? throw new NotFoundException($"Topic '{request.TopicId}' not found.");

        if (!topic.IsActive)
            throw new NotFoundException($"Topic '{request.TopicId}' not found.");

        if (request.GuestMode && !topic.IsDemo)
            throw new UnauthorizedException("Authentication is required to access this topic.");

        var lang = _lang.RequestedLanguage;
        var fallback = _lang.DefaultLanguage;

        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 50);

        var baseQuery = _db.Questions.AsNoTracking()
            .Where(q => q.TopicId == topic.Id && q.IsActive);

        var total = await baseQuery.CountAsync(ct);

        var rows = await baseQuery
            .OrderBy(q => q.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(q => new
            {
                q.Id,
                q.ImageKey,
                Translations = q.Translations
                    .Where(t => t.LanguageCode == lang || t.LanguageCode == fallback)
                    .Select(t => new { t.LanguageCode, t.Text, t.Explanation })
                    .ToList(),
                Answers = q.Answers
                    .OrderBy(a => a.OrderIndex)
                    .Select(a => new
                    {
                        a.Id,
                        a.OrderIndex,
                        a.IsCorrect,
                        Translations = a.Translations
                            .Where(t => t.LanguageCode == lang || t.LanguageCode == fallback)
                            .Select(t => new { t.LanguageCode, t.Text })
                            .ToList()
                    })
                    .ToList()
            })
            .ToListAsync(ct);

        var items = rows.Select(q =>
        {
            var preferredQ = q.Translations.FirstOrDefault(t => t.LanguageCode == lang);
            var actualQ = preferredQ ?? q.Translations.FirstOrDefault(t => t.LanguageCode == fallback);

            var answers = q.Answers.Select(a =>
            {
                var preferredA = a.Translations.FirstOrDefault(t => t.LanguageCode == lang);
                var actualA = preferredA ?? a.Translations.FirstOrDefault(t => t.LanguageCode == fallback);
                return new AnswerStudentDto(
                    a.Id,
                    a.OrderIndex,
                    a.IsCorrect,
                    actualA?.Text ?? "(no translation)",
                    actualA?.LanguageCode ?? fallback,
                    preferredA is null);
            }).ToList();

            return new QuestionStudentDto(
                q.Id,
                q.ImageKey,
                actualQ?.Text ?? "(no translation)",
                actualQ?.Explanation,
                actualQ?.LanguageCode ?? fallback,
                preferredQ is null,
                answers);
        }).ToList();

        return new PagedResult<QuestionStudentDto>(items, page, size, total);
    }
}