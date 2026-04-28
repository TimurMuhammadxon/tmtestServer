using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Infrastructure.Authentication;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    // Pre-computed hash, вычисляется один раз. Используется для constant-time login.
    private static readonly string _dummyHash =
        BCrypt.Net.BCrypt.HashPassword(string.Empty, WorkFactor);

    public string DummyHash => _dummyHash;

    public Task<string> HashAsync(string password, CancellationToken ct = default)
        => Task.Run(() => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor), ct);

    public Task<bool> VerifyAsync(string password, string hash, CancellationToken ct = default)
        => Task.Run(() =>
        {
            try { return BCrypt.Net.BCrypt.Verify(password, hash); }
            catch { return false; }
        }, ct);
}