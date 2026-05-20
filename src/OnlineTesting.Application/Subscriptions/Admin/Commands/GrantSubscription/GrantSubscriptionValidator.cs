using FluentValidation;

namespace OnlineTesting.Application.Subscriptions.Admin.Commands.GrantSubscription;

public class GrantSubscriptionValidator : AbstractValidator<GrantSubscriptionCommand>
{
    public GrantSubscriptionValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PlanId).NotEmpty();
    }
}
