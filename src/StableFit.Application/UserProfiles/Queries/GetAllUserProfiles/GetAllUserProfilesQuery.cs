using MediatR;
using StableFit.Application.UserProfiles.DTOs;
using StableFit.Application.Interfaces;

namespace StableFit.Application.UserProfiles.Queries.GetAllUserProfiles;

public sealed record GetAllUserProfilesQuery : IRequest<IReadOnlyList<UserProfileDto>>;

public sealed class GetAllUserProfilesQueryHandler : IRequestHandler<GetAllUserProfilesQuery, IReadOnlyList<UserProfileDto>>
{
    private readonly IUserProfileRepository _repository;

    public GetAllUserProfilesQueryHandler(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<UserProfileDto>> Handle(GetAllUserProfilesQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _repository.GetAllAsync(cancellationToken);
        
        return profiles.Select(p => new UserProfileDto(p.Id, p.Username, p.Name, p.Email)).ToList().AsReadOnly();
    }
}
