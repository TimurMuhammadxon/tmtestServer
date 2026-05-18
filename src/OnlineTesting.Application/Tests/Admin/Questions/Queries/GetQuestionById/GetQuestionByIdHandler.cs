using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Tests.Common;

namespace OnlineTesting.Application.Tests.Admin.Questions.Queries.GetQuestionById;

public class GetQuestionByIdHandler : IRequestHandler<GetQuestionByIdQuery, QuestionAdminDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageService _storage;

    public GetQuestionByIdHandler(IApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<QuestionAdminDto> Handle(GetQuestionByIdQuery request, CancellationToken ct)
    {
        var q = await _db.Questions
            .AsNoTracking()
            .Include(x => x.Translations)
            .Include(x => x.Answers).ThenInclude(a => a.Translations)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException($"Question '{request.Id}' not found.");

        return new QuestionAdminDto(
            q.Id,
            q.TopicId,
            q.ImageKey,
            q.ImageKey is not null ? _storage.GetPublicUrl(q.ImageKey) : null,
            q.IsActive,
            q.CreatedAt,
            q.UpdatedAt,
            q.Translations
                .OrderBy(t => t.LanguageCode)
                .Select(t => new QuestionTranslationDto(t.LanguageCode, t.Text, t.Explanation))
                .ToList(),
            q.Answers
                .OrderBy(a => a.OrderIndex)
                .Select(a => new AnswerAdminDto(
                    a.Id,
                    a.OrderIndex,
                    a.IsCorrect,
                    a.Translations
                        .OrderBy(t => t.LanguageCode)
                        .Select(t => new AnswerTranslationDto(t.LanguageCode, t.Text))
                        .ToList()))
                .ToList());
    }
}