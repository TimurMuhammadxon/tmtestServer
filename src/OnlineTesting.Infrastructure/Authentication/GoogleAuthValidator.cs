using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Infrastructure.Authentication;

public class GoogleAuthValidator : IGoogleAuthValidator
{
    private readonly GoogleOptions _options;

    public GoogleAuthValidator(IOptions<GoogleOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleAuthData> ValidateAsync(string idToken, CancellationToken ct = default)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_options.ClientId],
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch (InvalidJwtException)
        {
            throw new UnauthorizedException("Google authentication failed.");
        }

        return new GoogleAuthData(
            ExternalUserId: payload.Subject,
            Email: payload.Email,
            FirstName: payload.GivenName,
            LastName: payload.FamilyName);
    }
}
