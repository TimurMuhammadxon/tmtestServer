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

        question.UpdateBasics(request.TopicId, request.ImageKey);

        var drafts = request.Answers.Select(a => new AnswerDraft(
            a.OrderIndex,
            a.IsCorrect,
            a.Translations
                .Select(t => (t.LanguageCode.ToLowerInvariant(), t.Text))
                .ToList()
        )).ToList();

        question.ReplaceAnswers(drafts);

        await _db.SaveChangesAsync(ct);
    }
}