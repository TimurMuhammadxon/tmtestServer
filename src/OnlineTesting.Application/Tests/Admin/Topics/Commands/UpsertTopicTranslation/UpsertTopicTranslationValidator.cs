using FluentValidation;
using OnlineTesting.Application.Common.Constants;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.UpsertTopicTranslation;

public class UpsertTopicTranslationValidator : AbstractValidator<UpsertTopicTranslationCommand>
{
    public UpsertTopicTranslationValidator()
    {
        RuleFor(x => x.TopicId).NotEmpty();
        RuleFor(x => x.LanguageCode)
            .NotEmpty()
            .Must(Languages.IsSupported)
                .WithMessage($"Supported languages: {string.Join(", ", Languages.All)}.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}