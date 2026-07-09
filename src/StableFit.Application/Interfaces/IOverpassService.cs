using StableFit.Application.Gyms.DTOs;

namespace StableFit.Application.Interfaces;

public interface IOverpassService
{
    Task<IReadOnlyList<GymDto>> GetNearbyGymsAsync(
        double latitude,
        double longitude,
        int radiusMeters,
        CancellationToken cancellationToken = default);
}
