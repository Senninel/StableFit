using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using StableFit.Application.Gyms.DTOs;
using StableFit.Application.Interfaces;

namespace StableFit.Infrastructure.Services;

public sealed class OverpassService : IOverpassService
{
    private readonly HttpClient _httpClient;

    public OverpassService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<GymDto>> GetNearbyGymsAsync(
        double latitude,
        double longitude,
        int radiusMeters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(latitude, longitude, radiusMeters);
        using var requestContent = new StringContent($"data={Uri.EscapeDataString(query)}", Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await _httpClient.PostAsync("/api/interpreter", requestContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OverpassResponse>(cancellationToken);
        if (result?.Elements is null)
            return [];

        return result.Elements
            .Where(e => !string.IsNullOrWhiteSpace(e.Tags?.Name))
            .Select(ToGymDto)
            .ToList()
            .AsReadOnly();
    }

    private static string BuildQuery(double lat, double lng, int radius) =>
        $"""
         [out:json][timeout:25];
         (
           node["leisure"="fitness_centre"](around:{radius},{lat},{lng});
           way["leisure"="fitness_centre"](around:{radius},{lat},{lng});
           node["amenity"="gym"](around:{radius},{lat},{lng});
           way["amenity"="gym"](around:{radius},{lat},{lng});
         );
         out center;
         """;

    private static GymDto ToGymDto(OverpassElement element)
    {
        var tags = element.Tags!;
        var lat = element.Type == "way" ? element.Center?.Lat ?? 0 : element.Lat;
        var lon = element.Type == "way" ? element.Center?.Lon ?? 0 : element.Lon;

        return new GymDto
        {
            Name = tags.Name!,
            Address = BuildAddress(tags),
            Latitude = lat,
            Longitude = lon,
            OpeningHours = tags.OpeningHours,
            Website = tags.Website
        };
    }

    private static string BuildAddress(OverpassTags tags)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(tags.Street))
        {
            var line = string.IsNullOrWhiteSpace(tags.HouseNumber)
                ? tags.Street
                : $"{tags.Street} {tags.HouseNumber}";
            parts.Add(line);
        }

        if (!string.IsNullOrWhiteSpace(tags.City))
            parts.Add(tags.City);

        return string.Join(", ", parts);
    }

    private sealed class OverpassResponse
    {
        [JsonPropertyName("elements")]
        public List<OverpassElement> Elements { get; init; } = [];
    }

    private sealed class OverpassElement
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("lat")]
        public double Lat { get; init; }

        [JsonPropertyName("lon")]
        public double Lon { get; init; }

        [JsonPropertyName("center")]
        public OverpassCenter? Center { get; init; }

        [JsonPropertyName("tags")]
        public OverpassTags? Tags { get; init; }
    }

    private sealed class OverpassCenter
    {
        [JsonPropertyName("lat")]
        public double Lat { get; init; }

        [JsonPropertyName("lon")]
        public double Lon { get; init; }
    }

    private sealed class OverpassTags
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("addr:street")]
        public string? Street { get; init; }

        [JsonPropertyName("addr:housenumber")]
        public string? HouseNumber { get; init; }

        [JsonPropertyName("addr:city")]
        public string? City { get; init; }

        [JsonPropertyName("opening_hours")]
        public string? OpeningHours { get; init; }

        [JsonPropertyName("website")]
        public string? Website { get; init; }
    }
}
