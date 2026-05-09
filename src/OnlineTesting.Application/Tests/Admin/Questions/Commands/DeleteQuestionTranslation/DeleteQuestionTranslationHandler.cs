using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.DeleteQuestionTranslation;

public class DeleteQuestionTranslationHandler : IRequestHandler<DeleteQuestionTranslationCommand>
{
    private readonly IApplicationDbContext _db;
    public DeleteQuestionTranslationHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteQuestionTranslationCommand request, CancellationToken ct)
    {
        var question = await _db.Questions
            .Include(q => q.Translations)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, ct)
            ?? throw new NotFoundException($"Question '{request.QuestionId}' not found.");

        try
        {
            question.RemoveTranslation(request.LanguageCode);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await _db.SaveChangesAsync(ct);
    }
}