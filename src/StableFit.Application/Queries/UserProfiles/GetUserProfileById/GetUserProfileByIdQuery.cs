using MediatR;
using StableFit.Application.DTOs.UserProfiles;

namespace StableFit.Application.Queries.UserProfiles.GetUserProfileById;

public sealed record GetUserProfileByIdQuery(Guid Id) : IRequest<UserProfileDto?>;

