namespace StableFit.Application.Interfaces;

/// <summary>
/// Abstracts ASP.NET Core Identity user management operations.
/// Implemented in Infrastructure using UserManager&lt;AppUser&gt;.
/// </summary>
public interface IIdentityService
{
    Task<(bool Succeeded, string? UserId, IReadOnlyList<string> Errors)> RegisterAsync(
        string email, string password, string username, CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? UserId, string? Email, string? Username)> LoginAsync(
        string email, string password, CancellationToken cancellationToken = default);
}
