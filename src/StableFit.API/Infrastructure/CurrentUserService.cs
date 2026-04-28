using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using StableFit.Application.Interfaces;

namespace StableFit.API.Infrastructure;

/// <summary>
/// Reads the current authenticated user from HttpContext.
/// Registered as Scoped — one instance per HTTP request.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // JWT sub claim contains the user ID
    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
