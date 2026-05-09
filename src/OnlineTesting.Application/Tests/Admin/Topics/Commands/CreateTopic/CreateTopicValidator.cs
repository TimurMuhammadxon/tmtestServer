using FluentValidation;
using OnlineTesting.Application.Common.Constants;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.CreateTopic;

public class CreateTopicValidator : AbstractValidator<CreateTopicCommand>
{
    public CreateTopicValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().Length(2, 50)
            .Matches("^[a-z0-9-]+$").WithMessage("Code must be lowercase letters, digits, hyphens.");

        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);

        RuleFor(x => x.Translations)
            .NotEmpty()
            .Must(t => t.Select(x => x.LanguageCode.ToLowerInvariant()).Distinct().Count() == t.Count)
                .WithMessage("Translation language codes must be unique.")
            .Must(t => t.All(x => Languages.IsSupported(x.LanguageCode)))
                .WithMessage($"Supported languages: {string.Join(", ", Languages.All)}.")
            .Must(t => t.Any(x => string.Equals(x.LanguageCode, Languages.Default, StringComparison.OrdinalIgnoreCase)))
                .WithMessage($"Default language '{Languages.Default}' translation is required.");

        RuleForEach(x => x.Translations).ChildRules(t =>
        {
            t.RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        });
    }
}
