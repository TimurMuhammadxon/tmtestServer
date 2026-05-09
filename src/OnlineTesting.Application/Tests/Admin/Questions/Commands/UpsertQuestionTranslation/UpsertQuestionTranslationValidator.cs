using FluentValidation;
using OnlineTesting.Application.Common.Constants;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.UpsertQuestionTranslation;

public class UpsertQuestionTranslationValidator : AbstractValidator<UpsertQuestionTranslationCommand>
{
    public UpsertQuestionTranslationValidator()
    {
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.LanguageCode)
            .NotEmpty()
            .Must(Languages.IsSupported)
                .WithMessage($"Supported languages: {string.Join(", ", Languages.All)}.");
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Explanation).MaximumLength(4000);
    }
}