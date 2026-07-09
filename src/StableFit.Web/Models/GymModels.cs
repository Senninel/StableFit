namespace StableFit.Web.Models;

public record GymResult(
    string Name,
    string Address,
    double Latitude,
    double Longitude,
    string? OpeningHours,
    string? Website);

public class SetPreferredGymRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
