using StableFit.Domain;

namespace StableFit.Application.Interfaces;

public interface IUserProfileRepository
{
    Task AddAsync(UserProfile profile, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserProfile>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<UserProfile?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<UserProfile?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}

