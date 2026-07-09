using MediatR;
using StableFit.Application.Nutrition.DTOs;
using StableFit.Domain.Enums;

namespace StableFit.Application.Nutrition.Commands.AddNutritionEntry;

public sealed record AddNutritionEntryCommand : IRequest<NutritionEntryDto>
{
    public required MealType MealType { get; init; }
    public required string FoodName { get; init; }
    public required double Calories { get; init; }
    public required double ProteinGrams { get; init; }
    public required double CarbGrams { get; init; }
    public required double FatGrams { get; init; }
    public string? ServingDescription { get; init; }
    public DateOnly Date { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);
}
