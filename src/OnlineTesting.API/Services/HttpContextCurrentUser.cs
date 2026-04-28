using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.API.Services;

public class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public bool IsAuthenticated => _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            // MapInboundClaims = false → ищем напрямую по 'sub'.
            // Fallback на NameIdentifier — на случай, если кто-то отключит наш конфиг.
            var raw =
                _accessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }
}