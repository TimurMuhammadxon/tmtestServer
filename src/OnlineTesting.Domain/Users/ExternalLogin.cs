using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Users;

public class ExternalLogin : Entity
{
    public Guid UserId { get; private set; }
    public ExternalLoginProvider Provider { get; private set; }
    public string ExternalUserId { get; private set; } = string.Empty;
    public string? ExternalUsername { get; private set; }
    public DateTime LinkedAt { get; private set; }

    /// <summary>
    /// True when the bot can no longer message this Telegram user
    /// (blocked the bot / never started a private chat / deactivated).
    /// Such logins are skipped by the quiz broadcast.
    /// </summary>
    public bool TelegramBlocked { get; private set; }

    public User User { get; private set; } = null!;

    private ExternalLogin() { } // EF

    private ExternalLogin(
        Guid id,
        Guid userId,
        ExternalLoginProvider provider,
        string externalUserId,
        string? externalUsername) : base(id)
    {
        UserId = userId;
        Provider = provider;
        ExternalUserId = externalUserId;
        ExternalUsername = externalUsername;
        LinkedAt = DateTime.UtcNow;
    }

    public static ExternalLogin Link(
        Guid userId,
        ExternalLoginProvider provider,
        string externalUserId,
        string? externalUsername = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (string.IsNullOrWhiteSpace(externalUserId))
            throw new ArgumentException("ExternalUserId is required.", nameof(externalUserId));

        return new ExternalLogin(Guid.NewGuid(), userId, provider, externalUserId, externalUsername);
    }

    public void MarkTelegramBlocked() => TelegramBlocked = true;
}