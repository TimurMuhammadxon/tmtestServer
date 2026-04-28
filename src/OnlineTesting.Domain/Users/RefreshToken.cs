using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Users;

public class RefreshToken : Entity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public string? CreatedByIp { get; private set; }

    // PostgreSQL xmin — concurrency token, обновляется БД автоматически.
    public uint RowVersion { get; private set; }

    public User User { get; private set; } = null!;

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;

    private RefreshToken() { } // EF

    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string? createdByIp) : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        CreatedByIp = createdByIp;
    }

    public static RefreshToken Issue(Guid userId, string tokenHash, DateTime expiresAt, string? createdByIp = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("TokenHash is required.", nameof(tokenHash));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("ExpiresAt must be in the future.", nameof(expiresAt));

        return new RefreshToken(Guid.NewGuid(), userId, tokenHash, expiresAt, createdByIp);
    }

    public void Revoke(string? replacedByTokenHash = null)
    {
        if (RevokedAt is not null) return; // идемпотентно

        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}