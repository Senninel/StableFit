using Microsoft.AspNetCore.Identity;
using StableFit.Application.Interfaces;
using StableFit.Infrastructure.Identity;

namespace StableFit.Infrastructure.Services;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;

    public IdentityService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(bool Succeeded, string? UserId, IReadOnlyList<string> Errors)> RegisterAsync(
        string email, string password, string username, CancellationToken cancellationToken = default)
    {
        var user = new AppUser
        {
            Email = email,
            UserName = username,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList().AsReadOnly();
            return (false, null, errors);
        }

        return (true, user.Id, Array.Empty<string>());
    }

    public async Task<(bool Succeeded, string? UserId, string? Email, string? Username)> LoginAsync(
        string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return (false, null, null, null);

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
            return (false, null, null, null);

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return (true, user.Id, user.Email, user.UserName);
    }
}
