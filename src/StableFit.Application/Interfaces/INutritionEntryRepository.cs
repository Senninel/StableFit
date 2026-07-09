using StableFit.Domain.Entities;

namespace StableFit.Application.Interfaces;

public interface INutritionEntryRepository
{
    Task AddAsync(NutritionEntry entry, CancellationToken cancellationToken);

    Task<IReadOnlyList<NutritionEntry>> GetByUserIdAndDateAsync(string userId, DateOnly date, CancellationToken cancellationToken);

    Task<NutritionEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task DeleteAsync(NutritionEntry entry, CancellationToken cancellationToken);
}
