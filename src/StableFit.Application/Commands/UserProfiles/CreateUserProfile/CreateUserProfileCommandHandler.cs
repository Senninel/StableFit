using MediatR;
using StableFit.Application.DTOs.UserProfiles;
using StableFit.Application.Interfaces;
using StableFit.Domain.Entities;

namespace StableFit.Application.Commands.UserProfiles.CreateUserProfile;

public sealed class CreateUserProfileCommandHandler : IRequestHandler<CreateUserProfileCommand, UserProfileDto>
{
    private readonly IUserProfileRepository _repository;

    public CreateUserProfileCommandHandler(IUserProfileRepository repository)
    {
        _repository = repository;
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

        return new UserProfileDto(profile.Id, profile.Username, profile.Name, profile.Email);
    }
}

