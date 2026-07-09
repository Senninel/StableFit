using StableFit.Domain.Enums;

namespace StableFit.Domain.Entities;

public sealed class NutritionEntry
{
    public Guid Id { get; private set; }

    public string UserId { get; private set; }

    public DateOnly Date { get; private set; }

    public MealType MealType { get; private set; }

    public string FoodName { get; private set; }

    public double Calories { get; private set; }

    public double ProteinGrams { get; private set; }

    public double CarbGrams { get; private set; }

    public double FatGrams { get; private set; }

    public string? ServingDescription { get; private set; }

    private NutritionEntry(
        Guid id,
        string userId,
        DateOnly date,
        MealType mealType,
        string foodName,
        double calories,
        double proteinGrams,
        double carbGrams,
        double fatGrams,
        string? servingDescription)
    {
        Id = id;
        UserId = userId;
        Date = date;
        MealType = mealType;
        FoodName = foodName;
        Calories = calories;
        ProteinGrams = proteinGrams;
        CarbGrams = carbGrams;
        FatGrams = fatGrams;
        ServingDescription = servingDescription;
    }

    // For ORMs/serializers. Kept private to preserve invariants.
    private NutritionEntry()
    {
        UserId = string.Empty;
        FoodName = string.Empty;
    }

    public static NutritionEntry Create(
        string userId,
        DateOnly date,
        MealType mealType,
        string foodName,
        double calories,
        double proteinGrams,
        double carbGrams,
        double fatGrams,
        string? servingDescription = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (string.IsNullOrWhiteSpace(foodName))
            throw new ArgumentException("Food name is required.", nameof(foodName));

        if (calories < 0)
            throw new ArgumentException("Calories cannot be negative.", nameof(calories));

        return new NutritionEntry(
            Guid.NewGuid(),
            userId,
            date,
            mealType,
            foodName.Trim(),
            Math.Round(calories, 2),
            Math.Round(proteinGrams, 2),
            Math.Round(carbGrams, 2),
            Math.Round(fatGrams, 2),
            servingDescription?.Trim());
    }
}
