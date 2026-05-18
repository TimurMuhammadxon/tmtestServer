using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.DeleteQuestionImage;

public class DeleteQuestionImageHandler : IRequestHandler<DeleteQuestionImageCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageService _storage;

    public DeleteQuestionImageHandler(IApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task Handle(DeleteQuestionImageCommand request, CancellationToken ct)
    {
        var question = await _db.Questions.FirstOrDefaultAsync(q => q.Id == request.QuestionId, ct)
            ?? throw new NotFoundException($"Question '{request.QuestionId}' not found.");

        if (question.ImageKey is null)
            throw new NotFoundException($"Question '{request.QuestionId}' has no image.");

        await _storage.DeleteAsync(question.ImageKey, ct);
        question.RemoveImage();
        await _db.SaveChangesAsync(ct);
    }
}
