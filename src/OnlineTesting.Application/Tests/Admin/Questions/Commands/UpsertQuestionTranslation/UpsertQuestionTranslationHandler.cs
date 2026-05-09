using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.UpsertQuestionTranslation;

public class UpsertQuestionTranslationHandler : IRequestHandler<UpsertQuestionTranslationCommand>
{
    private readonly IApplicationDbContext _db;
    public UpsertQuestionTranslationHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpsertQuestionTranslationCommand request, CancellationToken ct)
    {
        var question = await _db.Questions
            .Include(q => q.Translations)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, ct)
            ?? throw new NotFoundException($"Question '{request.QuestionId}' not found.");

        question.UpsertTranslation(request.LanguageCode, request.Text, request.Explanation);
        await _db.SaveChangesAsync(ct);
    }
}