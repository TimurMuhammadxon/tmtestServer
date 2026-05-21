using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Payments;

public class PaymeTransaction : Entity
{
    public string PaymeTransactionId { get; private set; } = null!;
    public Guid OrderId { get; private set; }
    public long Amount { get; private set; }
    public PaymeTransactionState State { get; private set; }
    public long CreateTime { get; private set; }
    public long? PerformTime { get; private set; }
    public long? CancelTime { get; private set; }
    public int? CancelReason { get; private set; }

    private PaymeTransaction() { }

    public static PaymeTransaction Create(string paymeTransactionId, Guid orderId, long amount, long createTime) =>
        new()
        {
            Id = Guid.NewGuid(),
            PaymeTransactionId = paymeTransactionId,
            OrderId = orderId,
            Amount = amount,
            State = PaymeTransactionState.Created,
            CreateTime = createTime
        };

    public void Complete(long performTime)
    {
        State = PaymeTransactionState.Completed;
        PerformTime = performTime;
    }

    public void Cancel(long cancelTime, int reason)
    {
        State = State == PaymeTransactionState.Completed
            ? PaymeTransactionState.CancelledAfterCompletion
            : PaymeTransactionState.CancelledBeforeCompletion;
        CancelTime = cancelTime;
        CancelReason = reason;
    }
}
