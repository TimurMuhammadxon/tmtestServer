using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Settings;
using OnlineTesting.Domain.Payments;

namespace OnlineTesting.Application.Payments.Commands.InitiateClickPayment;

public class InitiateClickPaymentHandler : IRequestHandler<InitiateClickPaymentCommand, InitiateClickPaymentResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ClickSettings _click;

    public InitiateClickPaymentHandler(IApplicationDbContext db, ICurrentUser currentUser, IOptions<ClickSettings> click)
    {
        _db = db;
        _currentUser = currentUser;
        _click = click.Value;
    }

    public async Task<InitiateClickPaymentResult> Handle(InitiateClickPaymentCommand request, CancellationToken ct)
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

        // Store tiyins in PaymentOrder (consistent with Payme)
        var amountTiyin = (long)(plan.Price * 100);
        var order = PaymentOrder.Create(userId, plan.Id, amountTiyin);
        _db.PaymentOrders.Add(order);
        await _db.SaveChangesAsync(ct);

        var checkoutUrl = $"{_click.CheckoutUrl.TrimEnd('/')}" +
                          $"?service_id={_click.ServiceId}" +
                          $"&merchant_id={_click.MerchantId}" +
                          $"&amount={plan.Price.ToString("0.##", CultureInfo.InvariantCulture)}" +
                          $"&transaction_param={order.Id}";

        return new InitiateClickPaymentResult(order.Id, checkoutUrl, plan.Price);
    }
}
