using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Tests.Solutions.Attempts.Commands.FinishAttempt;

public class FinishAttemptHandler : IRequestHandler<FinishAttemptCommand, FinishAttemptResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public FinishAttemptHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<FinishAttemptResult> Handle(FinishAttemptCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var attempt = await _db.Attempts
            .Include(a => a.Questions)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, ct)
            ?? throw new NotFoundException($"Attempt '{request.AttemptId}' not found.");

        if (attempt.UserId != userId)
            throw new NotFoundException($"Attempt '{request.AttemptId}' not found.");

        if (attempt.Status != AttemptStatus.InProgress)
            throw new ConflictException("Attempt is already finished.");

        attempt.Finish();
        await _db.SaveChangesAsync(ct);

        return new FinishAttemptResult(
            attempt.Status.ToString(),
            attempt.CorrectCount!.Value,
            attempt.Questions.Count,
            attempt.FinishedAt!.Value);
    }
}
