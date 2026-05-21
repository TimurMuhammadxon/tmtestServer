using MediatR;

namespace OnlineTesting.Application.Payments.Commands.InitiatePayment;

public record InitiatePaymentCommand(Guid PlanId) : IRequest<InitiatePaymentResult>;

public record InitiatePaymentResult(Guid OrderId, string CheckoutUrl, long AmountTiyin);
