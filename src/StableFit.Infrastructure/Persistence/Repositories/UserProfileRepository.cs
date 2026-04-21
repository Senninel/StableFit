using Microsoft.EntityFrameworkCore;
using StableFit.Application.Interfaces;
using StableFit.Domain.Entities;

namespace StableFit.Infrastructure.Persistence.Repositories;

public sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly StableFitDbContext _db;

    public UserProfileRepository(StableFitDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(UserProfile profile, CancellationToken cancellationToken)
    {
        return _db.UserProfiles.AddAsync(profile, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<UserProfile>> GetAllAsync(CancellationToken cancellationToken)
        => await _db.UserProfiles.AsNoTracking().ToListAsync(cancellationToken);

    public Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<UserProfile?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var normalized = username.Trim();
        return _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.Username == normalized, cancellationToken);
    }

    public Task<UserProfile?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.Email == normalized, cancellationToken);
    }
}
