using Microsoft.EntityFrameworkCore;
using StableFit.Application.Interfaces;
using StableFit.Domain.Entities;

namespace StableFit.Infrastructure.Persistence.Repositories;

public sealed class NutritionEntryRepository : INutritionEntryRepository
{
    private readonly StableFitDbContext _db;

    public NutritionEntryRepository(StableFitDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(NutritionEntry entry, CancellationToken cancellationToken)
        => _db.NutritionEntries.AddAsync(entry, cancellationToken).AsTask();

    public async Task<IReadOnlyList<NutritionEntry>> GetByUserIdAndDateAsync(string userId, DateOnly date, CancellationToken cancellationToken)
        => await _db.NutritionEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date == date)
            .OrderBy(e => e.MealType)
            .ToListAsync(cancellationToken);

    public Task<NutritionEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _db.NutritionEntries.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task DeleteAsync(NutritionEntry entry, CancellationToken cancellationToken)
    {
        _db.NutritionEntries.Remove(entry);
        return Task.CompletedTask;
    }
}
