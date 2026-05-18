using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.DeleteQuestion;

public class DeleteQuestionHandler : IRequestHandler<DeleteQuestionCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageService _storage;

    public DeleteQuestionHandler(IApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task Handle(DeleteQuestionCommand request, CancellationToken ct)
    {
        var question = await _db.Questions.FirstOrDefaultAsync(q => q.Id == request.Id, ct)
            ?? throw new NotFoundException($"Question '{request.Id}' not found.");

        var usedInBilet = await _db.BiletQuestions
            .Where(bq => bq.QuestionId == request.Id)
            .Select(bq => bq.Bilet!.Number)
            .FirstOrDefaultAsync(ct);

        if (usedInBilet != 0)
            throw new ConflictException(
                $"Question is used in bilet #{usedInBilet}. Remove it from the bilet first.");

        if (question.ImageKey is not null)
            await _storage.DeleteAsync(question.ImageKey, ct);

        _db.Questions.Remove(question);
        await _db.SaveChangesAsync(ct);
    }
}