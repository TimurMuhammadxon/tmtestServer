using FluentValidation;

namespace OnlineTesting.Application.Payments.Commands.InitiatePayment;

public class InitiatePaymentValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
    }
}
