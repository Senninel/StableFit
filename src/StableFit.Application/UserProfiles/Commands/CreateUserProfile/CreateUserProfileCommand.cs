using MediatR;
using StableFit.Application.UserProfiles.DTOs;

namespace StableFit.Application.UserProfiles.Commands.CreateUserProfile;

public sealed record CreateUserProfileCommand(string Username, string Name, string Email) : IRequest<UserProfileDto>;
