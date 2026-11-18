using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Tests;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Tests.Solutions.Attempts.Commands.StartAttempt;

public class StartAttemptHandler : IRequestHandler<StartAttemptCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ISubscriptionChecker _subscription;

    public StartAttemptHandler(IApplicationDbContext db, ICurrentUser currentUser, ISubscriptionChecker subscription)
    {
        _db = db;
        _currentUser = currentUser;
        _subscription = subscription;
    }

    public async Task<Guid> Handle(StartAttemptCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var isDemoBilet = request.FlowType == FlowType.Bilet &&
            await _db.Bilets
                .Where(b => b.Id == request.BiletId!.Value)
                .Select(b => b.IsDemo)
                .FirstOrDefaultAsync(ct);

        var userRole = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Role)
            .FirstOrDefaultAsync(ct);

        var isPrivileged = userRole is Role.Teacher or Role.Admin or Role.SuperAdmin or Role.Owner;

        if (!isDemoBilet && !isPrivileged && !await _subscription.IsActiveAsync(userId, ct))
            throw new ConflictException("An active subscription is required to start this test.");

        var questionIds = request.FlowType switch
        {
            FlowType.Bilet    => await SelectBiletQuestionsAsync(request.BiletId!.Value, ct),
            FlowType.Topic    => await SelectAllTopicQuestionsAsync(request.TopicIds![0], ct),
            FlowType.Custom   => await SelectCustomQuestionsAsync(request.TopicIds, request.QuestionCount!.Value, ct),
            FlowType.Exam     => await SelectExamQuestionsAsync(ct),
            FlowType.Marathon => await SelectMarathonQuestionsAsync(ct),
            FlowType.Mistakes => await SelectMistakeQuestionsAsync(userId, ct),
            FlowType.NewQuestions => await SelectNewestQuestionsAsync(request.QuestionCount!.Value, ct),
            _                 => throw new ArgumentOutOfRangeException(nameof(request.FlowType))
        };

        if (questionIds.Count == 0)
            throw new ValidationException(new[] { new ValidationFailure("flowType", "No active questions found for the selected parameters.") });

        Guid? biletId = request.FlowType == FlowType.Bilet ? request.BiletId : null;
        var attempt = Attempt.Start(userId, request.FlowType, questionIds, biletId);

        _db.Attempts.Add(attempt);
        await _db.SaveChangesAsync(ct);

        return attempt.Id;
    }

    private async Task<IReadOnlyList<Guid>> SelectBiletQuestionsAsync(Guid biletId, CancellationToken ct)
    {
        var bilet = await _db.Bilets
            .Include(b => b.BiletQuestions.OrderBy(bq => bq.OrderIndex))
            .FirstOrDefaultAsync(b => b.Id == biletId, ct)
            ?? throw new NotFoundException($"Bilet '{biletId}' not found.");

        if (!bilet.IsActive)
            throw new ConflictException($"Bilet '{biletId}' is not active.");

        return bilet.BiletQuestions.Select(bq => bq.QuestionId).ToList();
    }

    private async Task<IReadOnlyList<Guid>> SelectAllTopicQuestionsAsync(Guid topicId, CancellationToken ct)
    {
        var exists = await _db.Topics.AnyAsync(t => t.Id == topicId, ct);
        if (!exists)
            throw new NotFoundException($"Topic '{topicId}' not found.");

        return await _db.Questions
            .Where(q => q.TopicId == topicId && q.IsActive)
            .OrderBy(_ => EF.Functions.Random())
            .Select(q => q.Id)
            .ToListAsync(ct);
    }

    private async Task<IReadOnlyList<Guid>> SelectCustomQuestionsAsync(
        IReadOnlyList<Guid>? topicIds, int count, CancellationToken ct)
    {
        var query = _db.Questions.Where(q => q.IsActive);

        if (topicIds != null && topicIds.Count > 0)
            query = query.Where(q => topicIds.Contains(q.TopicId));

        var available = await query.CountAsync(ct);
        if (available < count)
            throw new ValidationException(new[]
            {
                new ValidationFailure("questionCount", $"Not enough questions: requested {count}, available {available}.")
            });

        return await query
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .Select(q => q.Id)
            .ToListAsync(ct);
    }

    private async Task<IReadOnlyList<Guid>> SelectExamQuestionsAsync(CancellationToken ct)
    {
        var available = await _db.Questions.CountAsync(q => q.IsActive, ct);
        if (available < Attempt.ExamQuestionsCount)
            throw new ValidationException(new[]
            {
                new ValidationFailure("flowType", $"Not enough active questions for exam: need {Attempt.ExamQuestionsCount}, have {available}.")
            });

        return await _db.Questions
            .Where(q => q.IsActive)
            .OrderBy(_ => EF.Functions.Random())
            .Take(Attempt.ExamQuestionsCount)
            .Select(q => q.Id)
            .ToListAsync(ct);
    }

    private async Task<IReadOnlyList<Guid>> SelectMarathonQuestionsAsync(CancellationToken ct)
    {
        return await _db.Questions
            .Where(q => q.IsActive)
            .OrderBy(_ => EF.Functions.Random())
            .Select(q => q.Id)
            .ToListAsync(ct);
    }

    // Newest N active questions by creation date (for the "Yangi savollar" mode).
    private async Task<IReadOnlyList<Guid>> SelectNewestQuestionsAsync(int count, CancellationToken ct)
    {
        var ids = await _db.Questions
            .Where(q => q.IsActive)
            .OrderByDescending(q => q.CreatedAt)
            .Take(count)
            .Select(q => q.Id)
            .ToListAsync(ct);
        return ids.OrderBy(_ => Guid.NewGuid()).ToList();
    }

    // Active questions whose LATEST answer by this user was wrong ("unresolved" mistakes).
    // Answering one correctly here makes the correct answer the latest → it drops from the set.
    private async Task<IReadOnlyList<Guid>> SelectMistakeQuestionsAsync(Guid userId, CancellationToken ct)
    {
        var userAttemptIds = _db.Attempts
            .Where(a => a.UserId == userId && a.TestLinkId == null)
            .Select(a => a.Id);

        var answered = await _db.AttemptQuestions
            .Where(aq => userAttemptIds.Contains(aq.AttemptId) && aq.ChosenAnswerId != null && aq.AnsweredAt != null)
            .Select(aq => new { aq.QuestionId, aq.IsCorrect, aq.AnsweredAt })
            .ToListAsync(ct);

        var latestWrong = answered
            .GroupBy(a => a.QuestionId)
            .Where(g => g.OrderByDescending(x => x.AnsweredAt).First().IsCorrect == false)
            .Select(g => g.Key)
            .ToHashSet();

        if (latestWrong.Count == 0)
            return Array.Empty<Guid>();

        var activeWrong = await _db.Questions
            .Where(q => q.IsActive && latestWrong.Contains(q.Id))
            .Select(q => q.Id)
            .ToListAsync(ct);

        return activeWrong.OrderBy(_ => Guid.NewGuid()).ToList();
    }
}
