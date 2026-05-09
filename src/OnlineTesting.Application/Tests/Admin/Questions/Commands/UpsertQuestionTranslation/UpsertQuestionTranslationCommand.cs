using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.UpsertQuestionTranslation;

public record UpsertQuestionTranslationCommand(
    Guid QuestionId,
    string LanguageCode,
    string Text,
    string? Explanation) : IRequest;