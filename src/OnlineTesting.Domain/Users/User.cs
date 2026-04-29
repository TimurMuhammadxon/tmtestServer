using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Users;

public class User : Entity
{
    private readonly List<RefreshToken> _refreshTokens = new();
    private readonly List<ExternalLogin> _externalLogins = new();

    public string Email { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public Role Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    public IReadOnlyCollection<ExternalLogin> ExternalLogins => _externalLogins.AsReadOnly();

    private User() { } // EF

    private User(
        Guid id,
        string email,
        string? passwordHash,
        bool emailConfirmed,
        Role role) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        EmailConfirmed = emailConfirmed;
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Создание юзера через email + пароль. Email требует подтверждения.
    /// </summary>
    public static User CreateWithEmail(string email, string passwordHash, Role role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new User(
            Guid.NewGuid(),
            email.Trim().ToLowerInvariant(),
            passwordHash,
            emailConfirmed: false,
            role);
    }

    /// <summary>
    /// Создание юзера через external провайдера (Telegram и т.д.).
    /// Пароля нет, email — placeholder, EmailConfirmed = true (provider уже верифицировал identity).
    /// </summary>
    public static User CreateFromExternal(string placeholderEmail, Role role)
    {
        if (string.IsNullOrWhiteSpace(placeholderEmail))
            throw new ArgumentException("Placeholder email is required.", nameof(placeholderEmail));

        return new User(
            Guid.NewGuid(),
            placeholderEmail.Trim().ToLowerInvariant(),
            passwordHash: null,
            emailConfirmed: true,
            role);
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash is required.", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void ConfirmEmail() => EmailConfirmed = true;
}