using MediatR;
using StableFit.Application.DTOs.UserProfiles;
using StableFit.Application.Interfaces;

namespace StableFit.Application.Queries.UserProfiles.GetUserProfileById;

public sealed class GetUserProfileByIdQueryHandler : IRequestHandler<GetUserProfileByIdQuery, UserProfileDto?>
{
    private readonly IUserProfileRepository _repository;

    public GetUserProfileByIdQueryHandler(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserProfileDto?> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return profile is null ? null : new UserProfileDto(profile.Id, profile.Username, profile.Name, profile.Email);
    }
}

