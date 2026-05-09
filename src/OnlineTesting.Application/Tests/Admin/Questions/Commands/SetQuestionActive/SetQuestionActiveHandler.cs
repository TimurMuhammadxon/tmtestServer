using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.SetQuestionActive;

public class SetQuestionActiveHandler : IRequestHandler<SetQuestionActiveCommand>
{
    private readonly IApplicationDbContext _db;
    public SetQuestionActiveHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(SetQuestionActiveCommand request, CancellationToken ct)
    {
        var q = await _db.Questions.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException($"Question '{request.Id}' not found.");

        if (request.IsActive) q.Activate();
        else q.Deactivate();

        await _db.SaveChangesAsync(ct);
    }
}