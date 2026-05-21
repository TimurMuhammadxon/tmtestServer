using OnlineTesting.Application.Payments.Models;

namespace OnlineTesting.Application.Common.Interfaces;

public interface IClickWebhookProcessor
{
    Task<object> ProcessAsync(ClickWebhookRequest request, CancellationToken ct);
}
