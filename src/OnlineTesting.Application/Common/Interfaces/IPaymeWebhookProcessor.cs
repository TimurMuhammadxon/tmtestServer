using System.Text.Json;

namespace OnlineTesting.Application.Common.Interfaces;

public interface IPaymeWebhookProcessor
{
    Task<object> ProcessAsync(string method, JsonElement @params, CancellationToken ct);
}
