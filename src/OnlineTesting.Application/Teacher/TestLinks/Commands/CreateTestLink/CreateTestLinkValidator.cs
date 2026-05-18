using FluentValidation;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Teacher.TestLinks.Commands.CreateTestLink;

public class CreateTestLinkValidator : AbstractValidator<CreateTestLinkCommand>
{
    public CreateTestLinkValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);

        RuleFor(x => x.MaxAttempts).InclusiveBetween(1, 100);

        RuleFor(x => x.ExpiresAt).GreaterThan(DateTime.UtcNow)
            .WithMessage("ExpiresAt must be in the future.");

        RuleFor(x => x.BiletId).NotEmpty()
            .When(x => x.FlowType == FlowType.Bilet);

        RuleFor(x => x.TopicIds).NotEmpty()
            .When(x => x.FlowType == FlowType.Topic || x.FlowType == FlowType.Custom);

        RuleFor(x => x.QuestionCount).NotNull().GreaterThan(0)
            .When(x => x.FlowType == FlowType.Custom);
    }
}
