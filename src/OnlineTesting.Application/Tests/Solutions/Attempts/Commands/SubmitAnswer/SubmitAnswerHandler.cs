using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Progress;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Tests.Solutions.Attempts.Commands.SubmitAnswer;

public class SubmitAnswerHandler : IRequestHandler<SubmitAnswerCommand, SubmitAnswerResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SubmitAnswerHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<SubmitAnswerResult> Handle(SubmitAnswerCommand request, CancellationToken ct)
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

        var aq = attempt.Questions.FirstOrDefault(q => q.QuestionId == request.QuestionId)
            ?? throw new NotFoundException($"Question '{request.QuestionId}' is not part of this attempt.");

        if (aq.ChosenAnswerId.HasValue)
            throw new ConflictException("This question has already been answered.");

        var answer = await _db.Answers
            .FirstOrDefaultAsync(a => a.Id == request.AnswerId && a.QuestionId == request.QuestionId, ct)
            ?? throw new NotFoundException($"Answer '{request.AnswerId}' not found for this question.");

        var correctAnswerId = answer.IsCorrect
            ? answer.Id
            : await _db.Answers
                .Where(a => a.QuestionId == request.QuestionId && a.IsCorrect)
                .Select(a => a.Id)
                .FirstOrDefaultAsync(ct);

        var isFinished = attempt.Answer(request.QuestionId, request.AnswerId, answer.IsCorrect);

        await _db.SaveChangesAsync(ct);

        if (attempt.TestLinkId == null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var alreadyTracked = await _db.UserDailyActivities
                .AnyAsync(a => a.UserId == userId && a.ActivityDate == today, ct);
            if (!alreadyTracked)
            {
                try
                {
                    _db.UserDailyActivities.Add(UserDailyActivity.Create(userId, today));
                    await _db.SaveChangesAsync(ct);
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException)
                {
                    // concurrent request already inserted the same (user_id, activity_date) — safe to ignore
                }
            }
        }

        return new SubmitAnswerResult(
            answer.IsCorrect,
            correctAnswerId,
            isFinished,
            attempt.Status.ToString(),
            isFinished ? attempt.CorrectCount : null,
            attempt.Questions.Count);
    }
}
