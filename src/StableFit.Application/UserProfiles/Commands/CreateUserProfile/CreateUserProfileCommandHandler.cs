using MediatR;
using StableFit.Application.UserProfiles.DTOs;
using StableFit.Application.Interfaces;
using StableFit.Domain.Entities;

namespace StableFit.Application.UserProfiles.Commands.CreateUserProfile;

public sealed class CreateUserProfileCommandHandler : IRequestHandler<CreateUserProfileCommand, UserProfileDto>
{
    private readonly IUserProfileRepository _repository;
    private readonly IApplicationDbContext _dbContext;

    public CreateUserProfileCommandHandler(IUserProfileRepository repository, IApplicationDbContext dbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
    }

    public async Task<UserProfileDto> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var existingByUsername = await _repository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingByUsername is not null)
        {
            throw new InvalidOperationException("Username is already in use.");
        }

        var existingByEmail = await _repository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingByEmail is not null)
        {
            throw new InvalidOperationException("Email is already in use.");
        }

        var profile = UserProfile.Create(request.Username, request.Name, request.Email);
        await _repository.AddAsync(profile, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UserProfileDto(profile.Id, profile.Username, profile.Name, profile.Email);
    }
}
