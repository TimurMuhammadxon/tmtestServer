namespace OnlineTesting.Domain.Payments;

public enum PaymeTransactionState
{
    Created = 1,
    Completed = 2,
    CancelledBeforeCompletion = -1,
    CancelledAfterCompletion = -2
}
