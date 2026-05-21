using System.Text.Json.Serialization;

namespace OnlineTesting.Application.Payments.Models;

public class ClickWebhookRequest
{
    [JsonPropertyName("click_trans_id")]
    public long ClickTransId { get; set; }

    [JsonPropertyName("service_id")]
    public int ServiceId { get; set; }

    [JsonPropertyName("click_paydoc_id")]
    public long ClickPaydocId { get; set; }

    [JsonPropertyName("merchant_trans_id")]
    public string MerchantTransId { get; set; } = null!;

    [JsonPropertyName("merchant_prepare_id")]
    public long? MerchantPrepareId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("action")]
    public int Action { get; set; }

    [JsonPropertyName("error")]
    public int Error { get; set; }

    [JsonPropertyName("error_note")]
    public string ErrorNote { get; set; } = null!;

    [JsonPropertyName("sign_time")]
    public string SignTime { get; set; } = null!;

    [JsonPropertyName("sign_string")]
    public string SignString { get; set; } = null!;
}
