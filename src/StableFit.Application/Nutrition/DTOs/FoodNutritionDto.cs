namespace StableFit.Application.Nutrition.DTOs;

public sealed record FoodNutritionDto(
    string Name,
    double Calories,
    double ProteinGrams,
    double CarbGrams,
    double FatGrams);
