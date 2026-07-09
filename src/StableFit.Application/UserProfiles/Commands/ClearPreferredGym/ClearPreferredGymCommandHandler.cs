using MediatR;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;
using StableFit.Domain.Entities;

namespace StableFit.Application.UserProfiles.Commands.ClearPreferredGym;

public sealed class ClearPreferredGymCommandHandler : IRequestHandler<ClearPreferredGymCommand>
{
    private readonly IUserProfileRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public ClearPreferredGymCommandHandler(
        IUserProfileRepository repository,
        IApplicationDbContext dbContext,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task Handle(ClearPreferredGymCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var profile = await _repository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserProfile), userId);

        profile.ClearPreferredGym();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
