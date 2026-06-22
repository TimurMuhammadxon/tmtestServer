using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Progress.Queries.GetErrorQuestionDetail;

public class GetErrorQuestionDetailHandler : IRequestHandler<GetErrorQuestionDetailQuery, ErrorQuestionDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILanguageContext _lang;
    private readonly IStorageService _storage;

    public GetErrorQuestionDetailHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        ILanguageContext lang,
        IStorageService storage)
    {
        _db = db;
        _currentUser = currentUser;
        _lang = lang;
        _storage = storage;
    }

    public async Task<ErrorQuestionDetailDto> Handle(GetErrorQuestionDetailQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var question = await _db.Questions
            .Include(q => q.Translations)
            .Include(q => q.Answers).ThenInclude(a => a.Translations)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, ct)
            ?? throw new NotFoundException("Question not found.");

        var topic = await _db.Topics
            .Include(t => t.Translations)
            .FirstOrDefaultAsync(t => t.Id == question.TopicId, ct);

        var reqLang = _lang.RequestedLanguage;
        var defLang = _lang.DefaultLanguage;

        var questionText = question.Translations.FirstOrDefault(t => t.LanguageCode == reqLang)?.Text
            ?? question.Translations.FirstOrDefault(t => t.LanguageCode == defLang)?.Text
            ?? "(no translation)";

        var explanation = question.Translations.FirstOrDefault(t => t.LanguageCode == reqLang)?.Explanation
            ?? question.Translations.FirstOrDefault(t => t.LanguageCode == defLang)?.Explanation;

        var topicName = topic?.Translations.FirstOrDefault(t => t.LanguageCode == reqLang)?.Name
            ?? topic?.Translations.FirstOrDefault(t => t.LanguageCode == defLang)?.Name
            ?? "(no translation)";

        var imageUrl = question.ImageKey is not null ? _storage.GetPublicUrl(question.ImageKey) : null;

        var answers = question.Answers
            .OrderBy(a => a.OrderIndex)
            .Select(a => new ErrorAnswerDto(
                a.Id,
                a.Translations.FirstOrDefault(t => t.LanguageCode == reqLang)?.Text
                    ?? a.Translations.FirstOrDefault(t => t.LanguageCode == defLang)?.Text
                    ?? "(no translation)",
                a.IsCorrect))
            .ToList();

        var lastWrongAnswer = await _db.AttemptQuestions
            .Where(aq => aq.QuestionId == request.QuestionId
                && aq.IsCorrect == false
                && aq.ChosenAnswerId != null
                && _db.Attempts.Any(a => a.Id == aq.AttemptId && a.UserId == userId && a.TestLinkId == null))
            .OrderByDescending(aq => aq.AnsweredAt)
            .Select(aq => aq.ChosenAnswerId)
            .FirstOrDefaultAsync(ct);

        return new ErrorQuestionDetailDto(
            question.Id,
            questionText,
            imageUrl,
            explanation,
            topicName,
            answers,
            lastWrongAnswer);
    }
}
