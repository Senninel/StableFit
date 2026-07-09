using StableFit.Application.Nutrition.DTOs;

namespace StableFit.Application.Interfaces;

public interface INutritionApiService
{
    Task<IReadOnlyList<FoodSearchResultDto>> SearchFoodsAsync(string query, CancellationToken cancellationToken = default);

    Task<FoodNutritionDto?> GetFoodNutritionAsync(int spoonacularId, double amountGrams, CancellationToken cancellationToken = default);
}
