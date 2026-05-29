using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Common.Interfaces;

public interface IGoogleAuthValidator
{
    Task<GoogleAuthData> ValidateAsync(string idToken, CancellationToken ct = default);
}
