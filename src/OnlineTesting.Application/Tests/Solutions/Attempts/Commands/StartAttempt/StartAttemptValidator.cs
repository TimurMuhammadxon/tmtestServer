using FluentValidation;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Tests.Solutions.Attempts.Commands.StartAttempt;

public class StartAttemptValidator : AbstractValidator<StartAttemptCommand>
{
    public StartAttemptValidator()
    {
        RuleFor(x => x.FlowType)
            .IsInEnum().WithMessage("Invalid flow type.");

        When(x => x.FlowType == FlowType.Bilet, () =>
        {
            RuleFor(x => x.BiletId)
                .NotNull().WithMessage("BiletId is required for Bilet flow.")
                .NotEqual(Guid.Empty).WithMessage("BiletId cannot be empty.");
        });

        When(x => x.FlowType == FlowType.Topic, () =>
        {
            RuleFor(x => x.TopicIds)
                .NotNull().WithMessage("TopicIds is required for Topic flow.")
                .Must(ids => ids != null && ids.Count == 1)
                    .WithMessage("Exactly one TopicId is required for Topic flow.");
        });

        When(x => x.FlowType == FlowType.Custom, () =>
        {
            RuleFor(x => x.QuestionCount)
                .NotNull().WithMessage("QuestionCount is required for Custom flow.")
                .GreaterThan(0).WithMessage("QuestionCount must be positive.");
        });
    }
}
