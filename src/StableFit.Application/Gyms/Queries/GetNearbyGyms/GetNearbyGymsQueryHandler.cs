using MediatR;
using StableFit.Application.Gyms.DTOs;
using StableFit.Application.Interfaces;

namespace StableFit.Application.Gyms.Queries.GetNearbyGyms;

public sealed class GetNearbyGymsQueryHandler : IRequestHandler<GetNearbyGymsQuery, IReadOnlyList<GymDto>>
{
    private readonly IOverpassService _overpassService;

    public GetNearbyGymsQueryHandler(IOverpassService overpassService)
    {
        _overpassService = overpassService;
    }

    public Task<IReadOnlyList<GymDto>> Handle(GetNearbyGymsQuery request, CancellationToken cancellationToken)
    {
        return _overpassService.GetNearbyGymsAsync(
            request.Latitude,
            request.Longitude,
            request.RadiusMeters,
            cancellationToken);
    }
}
