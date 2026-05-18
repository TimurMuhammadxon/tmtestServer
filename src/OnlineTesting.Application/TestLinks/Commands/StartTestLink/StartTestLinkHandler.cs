using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.TestLinks.Commands.StartTestLink;

public class StartTestLinkHandler : IRequestHandler<StartTestLinkCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ISubscriptionChecker _subscription;

    public StartTestLinkHandler(IApplicationDbContext db, ICurrentUser currentUser, ISubscriptionChecker subscription)
    {
        _db = db;
        _currentUser = currentUser;
        _subscription = subscription;
    }

    public async Task<Guid> Handle(StartTestLinkCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var link = await _db.TestLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code == request.Code.ToUpper() && t.IsActive, ct)
            ?? throw new NotFoundException("Invalid or expired link.");

        if (link.ExpiresAt <= DateTime.UtcNow)
            throw new ConflictException("This link has expired.");

        if (!await _subscription.IsActiveAsync(userId, ct))
            throw new ConflictException("A paid subscription is required to use this link.");

        var attemptsUsed = await _db.Attempts
            .CountAsync(a => a.TestLinkId == link.Id && a.UserId == userId, ct);

        if (attemptsUsed >= link.MaxAttempts)
            throw new ConflictException($"You have reached the maximum number of attempts ({link.MaxAttempts}) for this link.");

        var questionIds = link.FlowType switch
        {
            FlowType.Bilet    => await SelectBiletQuestionsAsync(link.BiletId!.Value, ct),
            FlowType.Topic    => await SelectTopicQuestionsAsync(link.TopicIds[0], ct),
            FlowType.Custom   => await SelectCustomQuestionsAsync(link.TopicIds, link.QuestionCount!.Value, ct),
            FlowType.Exam     => await SelectExamQuestionsAsync(ct),
            FlowType.Marathon => await SelectMarathonQuestionsAsync(ct),
            _                 => throw new ArgumentOutOfRangeException(nameof(link.FlowType))
        };

        if (questionIds.Count == 0)
            throw new ValidationException(new[] { new ValidationFailure("flowType", "No active questions found for the selected parameters.") });

        Guid? biletId = link.FlowType == FlowType.Bilet ? link.BiletId : null;
        var attempt = Attempt.Start(userId, link.FlowType, questionIds, biletId, link.Id);

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

    private async Task<IReadOnlyList<Guid>> SelectTopicQuestionsAsync(Guid topicId, CancellationToken ct)
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
        List<Guid> topicIds, int count, CancellationToken ct)
    {
        var query = _db.Questions.Where(q => q.IsActive);

        if (topicIds.Count > 0)
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
}
