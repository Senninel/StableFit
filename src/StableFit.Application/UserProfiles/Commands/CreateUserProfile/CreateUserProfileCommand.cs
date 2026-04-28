using MediatR;
using StableFit.Application.UserProfiles.DTOs;

namespace StableFit.Application.UserProfiles.Commands.CreateUserProfile;

/// <remarks>
/// UserId must be the authenticated Identity user's ID.
/// TODO (Step 3): populate UserId from ICurrentUserService instead of passing it manually.
/// </remarks>
public sealed record CreateUserProfileCommand(
    string UserId,
    string Username,
    string Name,
    string Email) : IRequest<UserProfileDto>;
