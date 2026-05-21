using FluentValidation;

namespace OnlineTesting.Application.Payments.Commands.InitiateClickPayment;

public class InitiateClickPaymentValidator : AbstractValidator<InitiateClickPaymentCommand>
{
    public InitiateClickPaymentValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
    }
}
