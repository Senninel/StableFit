using MediatR;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;
using StableFit.Domain.Entities;

namespace StableFit.Application.UserProfiles.Commands.UpdateMyUserProfile;

public sealed class UpdateMyUserProfileCommandHandler : IRequestHandler<UpdateMyUserProfileCommand>
{
    private readonly IUserProfileRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public UpdateMyUserProfileCommandHandler(
        IUserProfileRepository repository,
        IApplicationDbContext dbContext,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateMyUserProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var profile = await _repository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserProfile), userId);

        profile.Update(request.Bio, request.Goal, request.ScheduleDays, request.AgeYears, request.WeightKg);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
