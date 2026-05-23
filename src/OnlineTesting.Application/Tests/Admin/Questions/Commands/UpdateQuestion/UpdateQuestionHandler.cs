using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.UpdateQuestion;

public class UpdateQuestionHandler : IRequestHandler<UpdateQuestionCommand>
{
    private readonly IApplicationDbContext _db;
    public UpdateQuestionHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateQuestionCommand request, CancellationToken ct)
    {
        var question = await _db.Questions
            .Include(q => q.Answers).ThenInclude(a => a.Translations)
            .Include(q => q.Translations)
            .FirstOrDefaultAsync(q => q.Id == request.Id, ct)
            ?? throw new NotFoundException($"Question '{request.Id}' not found.");

        var topicExists = await _db.Topics.AnyAsync(t => t.Id == request.TopicId, ct);
        if (!topicExists)
            throw new NotFoundException($"Topic '{request.TopicId}' not found.");

        var drafts = request.Answers.Select(a => new AnswerDraft(
            a.OrderIndex,
            a.IsCorrect,
            a.Translations
                .Select(t => (t.LanguageCode.ToLowerInvariant(), t.Text))
                .ToList()
        )).ToList();

        await using var tx = await _db.BeginTransactionAsync(ct);

        var oldAnswers = await _db.Answers
            .Where(a => a.QuestionId == request.Id)
            .ToListAsync(ct);

        _db.Answers.RemoveRange(oldAnswers);
        await _db.SaveChangesAsync(ct);

        question.UpdateBasics(request.TopicId, request.ImageKey);
        question.ReplaceAnswers(drafts);
        _db.Answers.AddRange(question.Answers);
        await _db.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);
    }
}
