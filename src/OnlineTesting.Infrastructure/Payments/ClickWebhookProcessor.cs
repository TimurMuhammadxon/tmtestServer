using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Settings;
using OnlineTesting.Application.Payments.Models;
using OnlineTesting.Domain.Payments;
using OnlineTesting.Domain.Subscriptions;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Infrastructure.Payments;

public class ClickWebhookProcessor : IClickWebhookProcessor
{
    private readonly IApplicationDbContext _db;
    private readonly ClickSettings _settings;
    private readonly IDbExceptionInspector _inspector;

    public ClickWebhookProcessor(IApplicationDbContext db, IOptions<ClickSettings> settings, IDbExceptionInspector inspector)
    {
        _db = db;
        _settings = settings.Value;
        _inspector = inspector;
    }

    public Task<object> ProcessAsync(ClickWebhookRequest req, CancellationToken ct) =>
        req.Action switch
        {
            0 => PrepareAsync(req, ct),
            1 => CompleteAsync(req, ct),
            _ => Task.FromResult(BuildError(req, 0, -3, "Action not found"))
        };

    private async Task<object> PrepareAsync(ClickWebhookRequest req, CancellationToken ct)
    {
        if (!VerifySign(req, null))
            return BuildError(req, 0, -1, "SIGN CHECK FAILED");

        if (!Guid.TryParse(req.MerchantTransId, out var orderId))
            return BuildError(req, 0, -6, "Transaction does not exist");

        var order = await _db.PaymentOrders.FirstOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
            return BuildError(req, 0, -5, "User does not exist");

        if (order.Status == PaymentOrderStatus.Cancelled)
            return BuildError(req, 0, -9, "Transaction cancelled");

        if (order.Status == PaymentOrderStatus.Paid)
            return BuildError(req, 0, -4, "Already paid");

        var expectedUzs = order.AmountTiyin / 100m;
        if (req.Amount != expectedUzs)
            return BuildError(req, 0, -2, "Incorrect parameter amount");

        // Idempotency: same click_trans_id already prepared
        var existing = await _db.ClickTransactions
            .FirstOrDefaultAsync(t => t.ClickTransactionId == req.ClickTransId.ToString(), ct);
        if (existing is not null)
            return BuildPrepareResult(req, existing.PrepareId);

        var tx = ClickTransaction.Prepare(req.ClickTransId.ToString(), orderId, req.Amount);
        _db.ClickTransactions.Add(tx);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (_inspector.IsUniqueConstraintViolation(ex))
        {
            // Race: another request inserted the same click_trans_id first
            var concurrent = await _db.ClickTransactions
                .FirstOrDefaultAsync(t => t.ClickTransactionId == req.ClickTransId.ToString(), ct);
            if (concurrent is not null)
                return BuildPrepareResult(req, concurrent.PrepareId);
            return BuildError(req, 0, -8, "Error in request from click");
        }

        return BuildPrepareResult(req, tx.PrepareId);
    }

    private async Task<object> CompleteAsync(ClickWebhookRequest req, CancellationToken ct)
    {
        if (!VerifySign(req, req.MerchantPrepareId?.ToString()))
            return BuildError(req, req.MerchantPrepareId ?? 0, -1, "SIGN CHECK FAILED");

        if (req.MerchantPrepareId is null)
            return BuildError(req, 0, -6, "Transaction does not exist");

        var tx = await _db.ClickTransactions
            .FirstOrDefaultAsync(t => t.PrepareId == req.MerchantPrepareId.Value, ct);
        if (tx is null)
            return BuildError(req, req.MerchantPrepareId.Value, -6, "Transaction does not exist");

        if (tx.State == ClickTransactionState.Completed)
            return BuildCompleteResult(req, tx.PrepareId);

        if (tx.State == ClickTransactionState.Cancelled)
            return BuildError(req, tx.PrepareId, -9, "Transaction cancelled");

        // Click signals cancellation via non-zero error
        if (req.Error != 0)
        {
            tx.Cancel(req.Error);
            var cancelledOrder = await _db.PaymentOrders.FirstOrDefaultAsync(o => o.Id == tx.OrderId, ct);
            cancelledOrder?.Cancel();
            await _db.SaveChangesAsync(ct);
            return BuildError(req, tx.PrepareId, -9, req.ErrorNote ?? "Cancelled");
        }

        var order = await _db.PaymentOrders.FirstOrDefaultAsync(o => o.Id == tx.OrderId, ct);
        if (order is null)
            return BuildError(req, tx.PrepareId, -5, "Order not found");

        if (order.Status == PaymentOrderStatus.Paid)
            return BuildCompleteResult(req, tx.PrepareId);

        if (order.Status == PaymentOrderStatus.Cancelled)
            return BuildError(req, tx.PrepareId, -9, "Transaction cancelled");

        // Grant before mutating state — if data integrity is violated, state stays Prepared and Click can retry
        try
        {
            await GrantSubscriptionAsync(order, ct);
        }
        catch (InvalidOperationException ex)
        {
            return BuildError(req, tx.PrepareId, -7, ex.Message);
        }

        tx.Complete();
        order.MarkPaid();
        await _db.SaveChangesAsync(ct);

        return BuildCompleteResult(req, tx.PrepareId);
    }

    private bool VerifySign(ClickWebhookRequest req, string? merchantPrepareId)
    {
        var amount = req.Amount.ToString("0.##", CultureInfo.InvariantCulture);
        var raw = req.Action == 0
            ? $"{req.ClickTransId}{_settings.ServiceId}{_settings.SecretKey}{req.MerchantTransId}{amount}{req.Action}{req.SignTime}"
            : $"{req.ClickTransId}{_settings.ServiceId}{_settings.SecretKey}{req.MerchantTransId}{merchantPrepareId}{amount}{req.Action}{req.SignTime}";

        var hash = MD5.HashData(Encoding.UTF8.GetBytes(raw));
        var computed = Convert.ToHexString(hash).ToLower();
        return computed == req.SignString;
    }

    private async Task GrantSubscriptionAsync(PaymentOrder order, CancellationToken ct)
    {
        var plan = await _db.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == order.PlanId, ct)
            ?? throw new InvalidOperationException("Subscription plan not found");

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == order.UserId, ct)
            ?? throw new InvalidOperationException("User not found");

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

    private static object BuildPrepareResult(ClickWebhookRequest req, long prepareId) =>
        new ClickPrepareResult(req.ClickTransId, req.MerchantTransId, prepareId, 0, "Success");

    private static object BuildCompleteResult(ClickWebhookRequest req, long prepareId) =>
        new ClickCompleteResult(req.ClickTransId, req.MerchantTransId, prepareId, 0, "Success");

    private static object BuildError(ClickWebhookRequest req, long prepareId, int error, string note) =>
        new ClickPrepareResult(req.ClickTransId, req.MerchantTransId, prepareId, error, note);
}

file record ClickPrepareResult(
    [property: JsonPropertyName("click_trans_id")]    long ClickTransId,
    [property: JsonPropertyName("merchant_trans_id")] string MerchantTransId,
    [property: JsonPropertyName("merchant_prepare_id")] long MerchantPrepareId,
    [property: JsonPropertyName("error")]             int Error,
    [property: JsonPropertyName("error_note")]        string ErrorNote);

file record ClickCompleteResult(
    [property: JsonPropertyName("click_trans_id")]     long ClickTransId,
    [property: JsonPropertyName("merchant_trans_id")]  string MerchantTransId,
    [property: JsonPropertyName("merchant_confirm_id")] long MerchantConfirmId,
    [property: JsonPropertyName("error")]              int Error,
    [property: JsonPropertyName("error_note")]         string ErrorNote);
