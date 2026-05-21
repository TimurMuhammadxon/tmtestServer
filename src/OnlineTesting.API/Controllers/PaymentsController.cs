using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Settings;
using OnlineTesting.Application.Payments.Commands.InitiateClickPayment;
using OnlineTesting.Application.Payments.Commands.InitiatePayment;
using OnlineTesting.Application.Payments.Models;

namespace OnlineTesting.API.Controllers;

[ApiController]
[Route("payments")]
public class PaymentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IPaymeWebhookProcessor _paymeProcessor;
    private readonly IClickWebhookProcessor _clickProcessor;
    private readonly PaymeSettings _payme;

    public PaymentsController(
        ISender sender,
        IPaymeWebhookProcessor paymeProcessor,
        IClickWebhookProcessor clickProcessor,
        IOptions<PaymeSettings> payme)
    {
        _sender         = sender;
        _paymeProcessor = paymeProcessor;
        _clickProcessor = clickProcessor;
        _payme          = payme.Value;
    }

    [HttpPost("payme/initiate")]
    [Authorize]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return Ok(result);
    }

    [HttpPost("payme/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymeWebhook([FromBody] JsonElement body, CancellationToken ct)
    {
        if (!ValidateBasicAuth())
            return BuildError(null, -32504, "Incorrect authorization data");

        if (!body.TryGetProperty("id", out var idEl)
            || !body.TryGetProperty("method", out var methodEl)
            || !body.TryGetProperty("params", out var paramsEl))
            return BuildError(null, -32600, "Invalid JSON-RPC request");

        var method = methodEl.GetString() ?? string.Empty;

        try
        {
            var result = await _paymeProcessor.ProcessAsync(method, paramsEl, ct);
            return Ok(new { id = idEl, result });
        }
        catch (PaymeRpcException ex)
        {
            return BuildError(idEl, ex.Code, ex.Message);
        }
    }

    private bool ValidateBasicAuth()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return false;

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader["Basic ".Length..]));
            var parts   = decoded.Split(':', 2);
            return parts.Length == 2 && parts[0] == "Paycom" && parts[1] == _payme.MerchantKey;
        }
        catch
        {
            return false;
        }
    }

    private IActionResult BuildError(JsonElement? id, int code, string message) =>
        Ok(new { id, error = new { code, message } });

    // ── Click ──────────────────────────────────────────────────────────────

    [HttpPost("click/initiate")]
    [Authorize]
    public async Task<IActionResult> ClickInitiate([FromBody] InitiateClickPaymentCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return Ok(result);
    }

    [HttpPost("click/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> ClickWebhook([FromBody] ClickWebhookRequest req, CancellationToken ct)
    {
        var result = await _clickProcessor.ProcessAsync(req, ct);
        return Ok(result);
    }
}
