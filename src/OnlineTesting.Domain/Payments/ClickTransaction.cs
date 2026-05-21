using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Payments;

public class ClickTransaction : Entity
{
    // Auto-incremented by DB — returned to Click as merchant_prepare_id
    public long PrepareId { get; private set; }
    public string ClickTransactionId { get; private set; } = null!;
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public ClickTransactionState State { get; private set; }
    public DateTime PrepareTime { get; private set; }
    public DateTime? CompleteTime { get; private set; }
    public int Error { get; private set; }

    private ClickTransaction() { }

    public static ClickTransaction Prepare(string clickTransactionId, Guid orderId, decimal amount) =>
        new()
        {
            Id = Guid.NewGuid(),
            ClickTransactionId = clickTransactionId,
            OrderId = orderId,
            Amount = amount,
            State = ClickTransactionState.Prepared,
            PrepareTime = DateTime.UtcNow
        };

    public void Complete()
    {
        State = ClickTransactionState.Completed;
        CompleteTime = DateTime.UtcNow;
    }

    public void Cancel(int error)
    {
        State = ClickTransactionState.Cancelled;
        Error = error;
    }
}
