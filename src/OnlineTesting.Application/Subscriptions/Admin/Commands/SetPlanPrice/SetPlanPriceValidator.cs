using FluentValidation;

namespace OnlineTesting.Application.Subscriptions.Admin.Commands.SetPlanPrice;

public class SetPlanPriceValidator : AbstractValidator<SetPlanPriceCommand>
{
    public SetPlanPriceValidator()
    {
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
