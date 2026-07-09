using MediatR;
using StableFit.Application.Nutrition.DTOs;

namespace StableFit.Application.Nutrition.Queries.SearchFood;

public sealed record SearchFoodQuery(string Query) : IRequest<IReadOnlyList<FoodSearchResultDto>>;
