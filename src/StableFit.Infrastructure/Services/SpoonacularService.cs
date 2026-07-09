using System.Net.Http.Json;
using System.Text.Json.Serialization;
using StableFit.Application.Interfaces;
using StableFit.Application.Nutrition.DTOs;

namespace StableFit.Infrastructure.Services;

public sealed class SpoonacularService : INutritionApiService
{
    private readonly HttpClient _httpClient;

    public SpoonacularService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<FoodSearchResultDto>> SearchFoodsAsync(string query, CancellationToken cancellationToken = default)
    {
        var url = $"/food/ingredients/search?query={Uri.EscapeDataString(query)}&number=10";
        var httpResponse = await _httpClient.GetAsync(url, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
            return [];

        var response = await httpResponse.Content.ReadFromJsonAsync<SpoonacularSearchResponse>(cancellationToken);

        if (response?.Results is null)
            return [];

        return response.Results
            .Select(r => new FoodSearchResultDto(
                r.Id,
                r.Name,
                r.Image is not null ? $"https://spoonacular.com/cdn/ingredients_100x100/{r.Image}" : null))
            .ToList()
            .AsReadOnly();
    }

    public async Task<FoodNutritionDto?> GetFoodNutritionAsync(int spoonacularId, double amountGrams, CancellationToken cancellationToken = default)
    {
        var url = $"/food/ingredients/{spoonacularId}/information?amount={amountGrams}&unit=grams";
        var httpResponse = await _httpClient.GetAsync(url, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
            return null;

        var response = await httpResponse.Content.ReadFromJsonAsync<SpoonacularIngredientResponse>(cancellationToken);

        if (response is null)
            return null;

        var nutrients = response.Nutrition?.Nutrients ?? [];

        return new FoodNutritionDto(
            response.Name,
            GetNutrientAmount(nutrients, "Calories"),
            GetNutrientAmount(nutrients, "Protein"),
            GetNutrientAmount(nutrients, "Carbohydrates"),
            GetNutrientAmount(nutrients, "Fat"));
    }

    private static double GetNutrientAmount(IEnumerable<SpoonacularNutrient> nutrients, string name)
        => nutrients.FirstOrDefault(n => n.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Amount ?? 0;

    private sealed record SpoonacularSearchResponse(
        [property: JsonPropertyName("results")] List<SpoonacularSearchResult> Results);

    private sealed record SpoonacularSearchResult(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("image")] string? Image);

    private sealed record SpoonacularIngredientResponse(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("nutrition")] SpoonacularNutrition? Nutrition);

    private sealed record SpoonacularNutrition(
        [property: JsonPropertyName("nutrients")] List<SpoonacularNutrient> Nutrients);

    private sealed record SpoonacularNutrient(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("amount")] double Amount,
        [property: JsonPropertyName("unit")] string Unit);
}
