using MediatR;
using StableFit.Application.DTOs.UserProfiles;

namespace StableFit.Application.Commands.UserProfiles.CreateUserProfile;

public sealed record CreateUserProfileCommand(string Username, string Name, string Email) : IRequest<UserProfileDto>;

