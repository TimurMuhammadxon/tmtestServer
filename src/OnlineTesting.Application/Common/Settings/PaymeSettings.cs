namespace OnlineTesting.Application.Common.Settings;

public class PaymeSettings
{
    public const string SectionName = "Payme";
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantKey { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = "https://checkout.paycom.uz";
}
