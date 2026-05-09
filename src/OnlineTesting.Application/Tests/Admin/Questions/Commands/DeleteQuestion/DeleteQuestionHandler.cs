using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.DeleteQuestion;

public class DeleteQuestionHandler : IRequestHandler<DeleteQuestionCommand>
{
    private readonly IApplicationDbContext _db;
    public DeleteQuestionHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteQuestionCommand request, CancellationToken ct)
    {
        var question = await _db.Questions.FirstOrDefaultAsync(q => q.Id == request.Id, ct)
            ?? throw new NotFoundException($"Question '{request.Id}' not found.");

        _db.Questions.Remove(question);
        await _db.SaveChangesAsync(ct);
    }
}