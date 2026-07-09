using StableFit.Domain.Enums;

namespace StableFit.Application.Nutrition.DTOs;

public sealed record NutritionEntryDto(
    Guid Id,
    MealType MealType,
    string FoodName,
    double Calories,
    double ProteinGrams,
    double CarbGrams,
    double FatGrams,
    string? ServingDescription,
    DateOnly Date);
