using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Payments;
using OnlineTesting.Domain.Subscriptions;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Infrastructure.Payments;

public class PaymeWebhookProcessor : IPaymeWebhookProcessor
{
    private readonly IApplicationDbContext _db;
    private readonly IDbExceptionInspector _inspector;

    public PaymeWebhookProcessor(IApplicationDbContext db, IDbExceptionInspector inspector)
    {
        _db = db;
        _inspector = inspector;
    }

    public Task<object> ProcessAsync(string method, JsonElement p, CancellationToken ct) => method switch
    {
        "CheckPerformTransaction" => CheckPerformAsync(p, ct),
        "CreateTransaction"       => CreateAsync(p, ct),
        "PerformTransaction"      => PerformAsync(p, ct),
        "CancelTransaction"       => CancelAsync(p, ct),
        "CheckTransaction"        => CheckAsync(p, ct),
        "GetStatement"            => GetStatementAsync(p, ct),
        _                         => throw new PaymeRpcException(-32601, "Method not found")
    };

    private async Task<object> CheckPerformAsync(JsonElement p, CancellationToken ct)
    {
        var (orderId, amount) = ParseOrderParams(p);
        await ValidateOrderAsync(orderId, amount, ct);
        return new { allow = true };
    }

    private async Task<object> CreateAsync(JsonElement p, CancellationToken ct)
    {
        var paymeId = p.GetProperty("id").GetString()!;
        var time    = p.GetProperty("time").GetInt64();
        var amount  = p.GetProperty("amount").GetInt64();
        var (orderId, _) = ParseOrderParams(p);

        var existing = await _db.PaymeTransactions
            .FirstOrDefaultAsync(t => t.PaymeTransactionId == paymeId, ct);

        if (existing is not null)
        {
            if (existing.State == PaymeTransactionState.Created)
                return ToCreateResult(existing);
            throw new PaymeRpcException(-31008, "Transaction is in an invalid state");
        }

        await ValidateOrderAsync(orderId, amount, ct);

        var tx = PaymeTransaction.Create(paymeId, orderId, amount, time);
        _db.PaymeTransactions.Add(tx);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (_inspector.IsUniqueConstraintViolation(ex))
        {
            // Race: another request inserted the same paymeId first
            var concurrent = await _db.PaymeTransactions
                .FirstOrDefaultAsync(t => t.PaymeTransactionId == paymeId, ct);
            if (concurrent?.State == PaymeTransactionState.Created)
                return ToCreateResult(concurrent);
            throw new PaymeRpcException(-31008, "Transaction is in an invalid state");
        }

        return ToCreateResult(tx);
    }

    private async Task<object> PerformAsync(JsonElement p, CancellationToken ct)
    {
        var paymeId = p.GetProperty("id").GetString()!;

        var tx = await _db.PaymeTransactions
            .FirstOrDefaultAsync(t => t.PaymeTransactionId == paymeId, ct)
            ?? throw new PaymeRpcException(-31003, "Transaction not found");

        if (tx.State == PaymeTransactionState.Completed)
            return ToPerformResult(tx);

        if (tx.State != PaymeTransactionState.Created)
            throw new PaymeRpcException(-31008, "Transaction cannot be performed");

        var order = await _db.PaymentOrders
            .FirstOrDefaultAsync(o => o.Id == tx.OrderId, ct)
            ?? throw new PaymeRpcException(-31050, "Order not found");

        var performTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Grant before mutating state — if plan/user missing, state stays Created and Payme can retry
        await GrantSubscriptionAsync(order, ct);

        tx.Complete(performTime);
        order.MarkPaid();
        await _db.SaveChangesAsync(ct);

        return ToPerformResult(tx);
    }

    private async Task<object> CancelAsync(JsonElement p, CancellationToken ct)
    {
        var paymeId = p.GetProperty("id").GetString()!;
        var reason  = p.GetProperty("reason").GetInt32();

        var tx = await _db.PaymeTransactions
            .FirstOrDefaultAsync(t => t.PaymeTransactionId == paymeId, ct)
            ?? throw new PaymeRpcException(-31003, "Transaction not found");

        if (tx.State == PaymeTransactionState.Completed)
            throw new PaymeRpcException(-31008, "Cannot cancel a completed transaction");

        if (tx.State is PaymeTransactionState.CancelledBeforeCompletion
                     or PaymeTransactionState.CancelledAfterCompletion)
            return ToCancelResult(tx);

        var cancelTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        tx.Cancel(cancelTime, reason);

        var order = await _db.PaymentOrders
            .FirstOrDefaultAsync(o => o.Id == tx.OrderId, ct);
        order?.Cancel();

        await _db.SaveChangesAsync(ct);

        return ToCancelResult(tx);
    }

    private async Task<object> CheckAsync(JsonElement p, CancellationToken ct)
    {
        var paymeId = p.GetProperty("id").GetString()!;

        var tx = await _db.PaymeTransactions
            .FirstOrDefaultAsync(t => t.PaymeTransactionId == paymeId, ct)
            ?? throw new PaymeRpcException(-31003, "Transaction not found");

        return new
        {
            create_time  = tx.CreateTime,
            perform_time = tx.PerformTime ?? 0L,
            cancel_time  = tx.CancelTime  ?? 0L,
            transaction  = tx.Id.ToString(),
            state        = (int)tx.State,
            reason       = tx.CancelReason
        };
    }

    private async Task<object> GetStatementAsync(JsonElement p, CancellationToken ct)
    {
        var from = p.GetProperty("from").GetInt64();
        var to   = p.GetProperty("to").GetInt64();

        var transactions = await _db.PaymeTransactions
            .Where(t => t.CreateTime >= from && t.CreateTime <= to)
            .OrderBy(t => t.CreateTime)
            .ToListAsync(ct);

        return new
        {
            transactions = transactions.Select(t => new
            {
                id           = t.PaymeTransactionId,
                time         = t.CreateTime,
                amount       = t.Amount,
                account      = new { order_id = t.OrderId.ToString() },
                create_time  = t.CreateTime,
                perform_time = t.PerformTime ?? 0L,
                cancel_time  = t.CancelTime  ?? 0L,
                transaction  = t.Id.ToString(),
                state        = (int)t.State,
                reason       = t.CancelReason
            }).ToArray()
        };
    }

    private async Task ValidateOrderAsync(Guid orderId, long amount, CancellationToken ct)
    {
        var order = await _db.PaymentOrders.FirstOrDefaultAsync(o => o.Id == orderId, ct);

        if (order is null || order.Status == PaymentOrderStatus.Cancelled)
            throw new PaymeRpcException(-31050, "Order not found");
        if (order.Status == PaymentOrderStatus.Paid)
            throw new PaymeRpcException(-31051, "Order already paid");
        if (order.AmountTiyin != amount)
            throw new PaymeRpcException(-31001, "Incorrect amount");
    }

    private async Task GrantSubscriptionAsync(PaymentOrder order, CancellationToken ct)
    {
        var plan = await _db.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == order.PlanId, ct)
            ?? throw new PaymeRpcException(-31050, "Subscription plan not found");

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == order.UserId, ct)
            ?? throw new PaymeRpcException(-31050, "User not found");

        var existing = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == order.UserId, ct);

        var baseDate = existing is not null && existing.ExpiresAt > DateTime.UtcNow
            ? existing.ExpiresAt
            : DateTime.UtcNow;

        var newExpiresAt = plan.Duration switch
        {
            SubscriptionDuration.TwoWeeks    => baseDate.AddDays(14),
            SubscriptionDuration.OneMonth    => baseDate.AddMonths(1),
            SubscriptionDuration.TwoMonths   => baseDate.AddMonths(2),
            SubscriptionDuration.ThreeMonths => baseDate.AddMonths(3),
            _ => throw new ArgumentOutOfRangeException(nameof(plan.Duration))
        };

        if (existing is null)
            _db.Subscriptions.Add(Subscription.Create(order.UserId, plan.Id, newExpiresAt));
        else
            existing.Extend(plan.Id, newExpiresAt);

        if (plan.Type == SubscriptionPlanType.Teacher && user.Role == Role.Student)
            user.SetRole(Role.Teacher);
    }

    private static (Guid orderId, long amount) ParseOrderParams(JsonElement p)
    {
        var amount     = p.GetProperty("amount").GetInt64();
        var account    = p.GetProperty("account");
        var orderIdStr = account.GetProperty("order_id").GetString()
            ?? throw new PaymeRpcException(-31050, "Missing order_id");

        if (!Guid.TryParse(orderIdStr, out var orderId))
            throw new PaymeRpcException(-31050, "Invalid order_id");

        return (orderId, amount);
    }

    private static object ToCreateResult(PaymeTransaction tx) => new
    {
        create_time = tx.CreateTime,
        transaction = tx.Id.ToString(),
        state       = (int)tx.State
    };

    private static object ToPerformResult(PaymeTransaction tx) => new
    {
        perform_time = tx.PerformTime ?? 0L,
        transaction  = tx.Id.ToString(),
        state        = (int)tx.State
    };

    private static object ToCancelResult(PaymeTransaction tx) => new
    {
        cancel_time = tx.CancelTime ?? 0L,
        transaction = tx.Id.ToString(),
        state       = (int)tx.State
    };
}
