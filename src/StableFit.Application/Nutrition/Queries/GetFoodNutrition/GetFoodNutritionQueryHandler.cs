using MediatR;
using StableFit.Application.Interfaces;
using StableFit.Application.Nutrition.DTOs;

namespace StableFit.Application.Nutrition.Queries.GetFoodNutrition;

public sealed class GetFoodNutritionQueryHandler : IRequestHandler<GetFoodNutritionQuery, FoodNutritionDto?>
{
    private readonly INutritionApiService _nutritionApi;

    public GetFoodNutritionQueryHandler(INutritionApiService nutritionApi)
    {
        _nutritionApi = nutritionApi;
    }

    public Task<FoodNutritionDto?> Handle(GetFoodNutritionQuery request, CancellationToken cancellationToken)
        => _nutritionApi.GetFoodNutritionAsync(request.SpoonacularId, request.AmountGrams, cancellationToken);
}
