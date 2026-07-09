namespace StableFit.Application.Gyms.DTOs;

public record GymDto
{
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string? OpeningHours { get; init; }
    public string? Website { get; init; }
}
