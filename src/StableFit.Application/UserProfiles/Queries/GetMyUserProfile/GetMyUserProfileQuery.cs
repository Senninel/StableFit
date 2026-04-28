using MediatR;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;
using StableFit.Application.UserProfiles.DTOs;
using StableFit.Domain.Entities;

namespace StableFit.Application.UserProfiles.Queries.GetMyUserProfile;

public sealed record GetMyUserProfileQuery : IRequest<UserProfileDto>;

public sealed class GetMyUserProfileQueryHandler : IRequestHandler<GetMyUserProfileQuery, UserProfileDto>
{
    private readonly IUserProfileRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetMyUserProfileQueryHandler(IUserProfileRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<UserProfileDto> Handle(GetMyUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var profile = await _repository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserProfile), userId);

        return MapToDto(profile);
    }

    private static UserProfileDto MapToDto(UserProfile p) =>
        new(p.Id, p.UserId, p.Username, p.Name, p.Email,
            p.Bio, p.Goal, p.ScheduleDays, p.AgeYears, p.WeightKg);
}
