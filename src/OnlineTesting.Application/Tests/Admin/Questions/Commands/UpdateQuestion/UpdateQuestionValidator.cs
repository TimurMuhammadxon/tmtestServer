using FluentValidation;
using OnlineTesting.Application.Common.Constants;

namespace OnlineTesting.Application.Tests.Admin.Questions.Commands.UpdateQuestion;

public class UpdateQuestionValidator : AbstractValidator<UpdateQuestionCommand>
{
    public UpdateQuestionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TopicId).NotEmpty();
        RuleFor(x => x.ImageKey).MaximumLength(500);

        RuleFor(x => x.Answers)
            .NotEmpty()
            .Must(a => a.Count is >= 2 and <= 6).WithMessage("Question must have 2..6 answers.")
            .Must(a => a.Count(x => x.IsCorrect) == 1).WithMessage("Exactly one answer must be correct.")
            .Must(a => a.Select(x => x.OrderIndex).Distinct().Count() == a.Count)
                .WithMessage("Answer OrderIndex must be unique.");

        RuleForEach(x => x.Answers).ChildRules(a =>
        {
            a.RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
            a.RuleFor(x => x.Translations)
                .NotEmpty()
                .Must(t => t.Select(x => x.LanguageCode.ToLowerInvariant()).Distinct().Count() == t.Count)
                    .WithMessage("Answer translation language codes must be unique.")
                .Must(t => t.All(x => Languages.IsSupported(x.LanguageCode)))
                    .WithMessage($"Supported languages: {string.Join(", ", Languages.All)}.")
                .Must(t => t.Any(x => string.Equals(x.LanguageCode, Languages.Default, StringComparison.OrdinalIgnoreCase)))
                    .WithMessage($"Default language '{Languages.Default}' translation is required for each answer.");

            a.RuleForEach(x => x.Translations).ChildRules(t =>
            {
                t.RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
            });
        });
    }
}