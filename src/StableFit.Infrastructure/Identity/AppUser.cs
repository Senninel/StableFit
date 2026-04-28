using Microsoft.AspNetCore.Identity;

namespace StableFit.Infrastructure.Identity;

/// <summary>
/// Application-specific Identity user. Extends IdentityUser with no extra fields for MVP.
/// Additional profile data lives in UserProfile, linked via AppUser.Id.
/// </summary>
public sealed class AppUser : IdentityUser
{
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
