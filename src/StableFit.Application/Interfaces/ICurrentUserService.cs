namespace StableFit.Application.Interfaces;

/// <summary>
/// Provides the current authenticated user's identity.
/// Implemented in the API layer via IHttpContextAccessor.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    bool IsAuthenticated { get; }
}
