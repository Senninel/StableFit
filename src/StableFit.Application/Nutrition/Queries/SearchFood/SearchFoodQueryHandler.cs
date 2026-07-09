using MediatR;
using StableFit.Application.Interfaces;
using StableFit.Application.Nutrition.DTOs;

namespace StableFit.Application.Nutrition.Queries.SearchFood;

public sealed class SearchFoodQueryHandler : IRequestHandler<SearchFoodQuery, IReadOnlyList<FoodSearchResultDto>>
{
    private readonly INutritionApiService _nutritionApi;

    public SearchFoodQueryHandler(INutritionApiService nutritionApi)
    {
        _nutritionApi = nutritionApi;
    }

    public Task<IReadOnlyList<FoodSearchResultDto>> Handle(SearchFoodQuery request, CancellationToken cancellationToken)
        => _nutritionApi.SearchFoodsAsync(request.Query, cancellationToken);
}
