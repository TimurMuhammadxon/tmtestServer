using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Payments;

public class PaymentOrder : Entity
{
    public Guid UserId { get; private set; }
    public Guid PlanId { get; private set; }
    public long AmountTiyin { get; private set; }
    public PaymentOrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PaymentOrder() { }

    public static PaymentOrder Create(Guid userId, Guid planId, long amountTiyin) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = planId,
            AmountTiyin = amountTiyin,
            Status = PaymentOrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

    public void MarkPaid() => Status = PaymentOrderStatus.Paid;
    public void Cancel() => Status = PaymentOrderStatus.Cancelled;
}
