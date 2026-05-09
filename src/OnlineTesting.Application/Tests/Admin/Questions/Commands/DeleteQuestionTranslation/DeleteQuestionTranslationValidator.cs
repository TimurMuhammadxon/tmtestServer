using FluentValidation;
using OnlineTesting.Application.Common.Constants;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.DeleteQuestionTranslation;

public class DeleteQuestionTranslationValidator : AbstractValidator<DeleteQuestionTranslationCommand>
{
    public DeleteQuestionTranslationValidator()
    {
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.LanguageCode)
            .NotEmpty()
            .Must(c => !string.Equals(c, Languages.Default, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"Cannot delete translation in default language '{Languages.Default}'.")
            .Must(Languages.IsSupported)
                .WithMessage($"Supported languages: {string.Join(", ", Languages.All)}.");
    }
}