using MediatR;
using StableFit.Application.Gyms.DTOs;

namespace StableFit.Application.Gyms.Queries.GetNearbyGyms;

public record GetNearbyGymsQuery : IRequest<IReadOnlyList<GymDto>>
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public int RadiusMeters { get; init; } = 2000;
}
