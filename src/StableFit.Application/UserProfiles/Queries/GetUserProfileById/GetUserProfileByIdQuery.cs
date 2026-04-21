using MediatR;
using StableFit.Application.UserProfiles.DTOs;

namespace StableFit.Application.UserProfiles.Queries.GetUserProfileById;

public sealed record GetUserProfileByIdQuery(Guid Id) : IRequest<UserProfileDto?>;
