using MediatR;
using StableFit.Application.Nutrition.DTOs;

namespace StableFit.Application.Nutrition.Queries.GetFoodNutrition;

public sealed record GetFoodNutritionQuery(int SpoonacularId, double AmountGrams) : IRequest<FoodNutritionDto?>;
