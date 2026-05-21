using MediatR;

namespace OnlineTesting.Application.Payments.Commands.InitiateClickPayment;

public record InitiateClickPaymentCommand(Guid PlanId) : IRequest<InitiateClickPaymentResult>;

public record InitiateClickPaymentResult(Guid OrderId, string CheckoutUrl, decimal AmountUzs);
