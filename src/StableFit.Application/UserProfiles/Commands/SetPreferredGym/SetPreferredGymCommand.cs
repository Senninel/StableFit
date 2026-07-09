using MediatR;

namespace StableFit.Application.UserProfiles.Commands.SetPreferredGym;

public sealed record SetPreferredGymCommand : IRequest
{
    public required string Name { get; init; }
    public string? Address { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
