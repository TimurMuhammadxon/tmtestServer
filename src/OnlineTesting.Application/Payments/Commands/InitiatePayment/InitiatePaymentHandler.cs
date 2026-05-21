using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Settings;
using OnlineTesting.Domain.Payments;

namespace OnlineTesting.Application.Payments.Commands.InitiatePayment;

public class InitiatePaymentHandler : IRequestHandler<InitiatePaymentCommand, InitiatePaymentResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly PaymeSettings _payme;

    public InitiatePaymentHandler(IApplicationDbContext db, ICurrentUser currentUser, IOptions<PaymeSettings> payme)
    {
        _db = db;
        _currentUser = currentUser;
        _payme = payme.Value;
    }

    public async Task<InitiatePaymentResult> Handle(InitiatePaymentCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var plan = await _db.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, ct)
            ?? throw new NotFoundException($"Subscription plan '{request.PlanId}' not found.");

        if (!plan.IsActive)
            throw new ConflictException("Subscription plan is not active.");

        if (plan.Price <= 0)
            throw new ConflictException("Subscription plan has no price set.");

        var amountTiyin = (long)(plan.Price * 100);

        var order = PaymentOrder.Create(userId, plan.Id, amountTiyin);
        _db.PaymentOrders.Add(order);
        await _db.SaveChangesAsync(ct);

        var checkoutUrl = BuildCheckoutUrl(order.Id, amountTiyin);

        return new InitiatePaymentResult(order.Id, checkoutUrl, amountTiyin);
    }

    private string BuildCheckoutUrl(Guid orderId, long amountTiyin)
    {
        var raw = $"m={_payme.MerchantId};ac.order_id={orderId};a={amountTiyin}";
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        return $"{_payme.CheckoutUrl.TrimEnd('/')}/{encoded}";
    }
}
