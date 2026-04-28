namespace OnlineTesting.Application.Common.Interfaces;

public interface IPasswordHasher
{
    Task<string> HashAsync(string password, CancellationToken ct = default);
    Task<bool> VerifyAsync(string password, string hash, CancellationToken ct = default);

    /// <summary>
    /// Pre-computed hash, used for constant-time login when user does not exist.
    /// </summary>
    string DummyHash { get; }
}