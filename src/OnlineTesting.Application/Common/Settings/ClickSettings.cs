namespace OnlineTesting.Application.Common.Settings;

public class ClickSettings
{
    public const string SectionName = "Click";
    public string ServiceId { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = "https://my.click.uz/services/pay";
}
