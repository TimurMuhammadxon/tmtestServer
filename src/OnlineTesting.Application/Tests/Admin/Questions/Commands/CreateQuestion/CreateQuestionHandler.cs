using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.CreateQuestion;

public class CreateQuestionHandler : IRequestHandler<CreateQuestionCommand, CreateQuestionResponse>
{
    private readonly IApplicationDbContext _db;
    public CreateQuestionHandler(IApplicationDbContext db) => _db = db;

    public async Task<CreateQuestionResponse> Handle(CreateQuestionCommand request, CancellationToken ct)
    {
        var topicExists = await _db.Topics.AnyAsync(t => t.Id == request.TopicId, ct);
        if (!topicExists)
            throw new NotFoundException($"Topic '{request.TopicId}' not found.");

        var qTranslations = request.Translations
            .Select(t => (t.LanguageCode.ToLowerInvariant(), t.Text, t.Explanation))
            .ToList();

        var answers = request.Answers.Select(a => new AnswerDraft(
            a.OrderIndex,
            a.IsCorrect,
            a.Translations
                .Select(t => (t.LanguageCode.ToLowerInvariant(), t.Text))
                .ToList()
        )).ToList();

        var question = Question.Create(request.TopicId, request.ImageKey, qTranslations, answers);

        _db.Questions.Add(question);
        await _db.SaveChangesAsync(ct);

        return new CreateQuestionResponse(question.Id);
    }
}