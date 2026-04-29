namespace OnlineTesting.Infrastructure.Authentication;

public class TelegramOptions
{
    public const string SectionName = "Telegram";

    public string BotToken { get; init; } = string.Empty;
    public int AuthDateExpirationHours { get; init; } = 24;
}