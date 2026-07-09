using System;
using System.Collections.Generic;

namespace StableFit.Web.Models;

public enum MealType
{
    Breakfast = 0,
    Lunch = 1,
    Dinner = 2,
    Snack = 3
}

public enum BiologicalSex
{
    Male = 0,
    Female = 1,
    PreferNotToSay = 2
}

public class NutritionEntryDto
{
    public Guid Id { get; set; }
    public MealType MealType { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public double Calories { get; set; }
    public double ProteinGrams { get; set; }
    public double CarbGrams { get; set; }
    public double FatGrams { get; set; }
    public string? ServingDescription { get; set; }
    public DateOnly Date { get; set; }
}

public class FoodSearchResultDto
{
    public int SpoonacularId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class FoodNutritionDto
{
    public string Name { get; set; } = string.Empty;
    public double Calories { get; set; }
    public double ProteinGrams { get; set; }
    public double CarbGrams { get; set; }
    public double FatGrams { get; set; }
}

public class AddNutritionEntryRequest
{
    public MealType MealType { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public double Calories { get; set; }
    public double ProteinGrams { get; set; }
    public double CarbGrams { get; set; }
    public double FatGrams { get; set; }
    public string? ServingDescription { get; set; }
    public DateOnly? Date { get; set; }
}
