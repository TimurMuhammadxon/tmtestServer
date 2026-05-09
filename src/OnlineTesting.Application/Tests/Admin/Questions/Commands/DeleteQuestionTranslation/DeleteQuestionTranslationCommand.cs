using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.DeleteQuestionTranslation;

public record DeleteQuestionTranslationCommand(
    Guid QuestionId,
    string LanguageCode) : IRequest;