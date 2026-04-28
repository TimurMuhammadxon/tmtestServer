namespace OnlineTesting.Application.Common.Interfaces;

public interface IRefreshTokenService
{
    (string Raw, string Hash) Generate();
    string Hash(string raw);
    TimeSpan Lifetime { get; }
}