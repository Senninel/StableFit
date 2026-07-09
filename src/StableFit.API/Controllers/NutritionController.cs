using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StableFit.Application.Nutrition.Commands.AddNutritionEntry;
using StableFit.Application.Nutrition.Commands.DeleteNutritionEntry;
using StableFit.Application.Nutrition.DTOs;
using StableFit.Application.Nutrition.Queries.GetDiaryEntries;
using StableFit.Application.Nutrition.Queries.GetFoodNutrition;
using StableFit.Application.Nutrition.Queries.SearchFood;
using StableFit.Domain.Enums;

namespace StableFit.API.Controllers;

[ApiController]
[Route("api/nutrition")]
[Authorize]
public sealed class NutritionController : ControllerBase
{
    private readonly ISender _sender;

    public NutritionController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Searches Spoonacular for food ingredients matching the query.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchFood([FromQuery] string query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query is required.");

        var results = await _sender.Send(new SearchFoodQuery(query), ct);
        return Ok(results);
    }

    /// <summary>Returns nutrition data for a Spoonacular ingredient at a given gram amount.</summary>
    [HttpGet("food/{spoonacularId:int}")]
    public async Task<IActionResult> GetFoodNutrition(int spoonacularId, [FromQuery] double grams = 100, CancellationToken ct = default)
    {
        var nutrition = await _sender.Send(new GetFoodNutritionQuery(spoonacularId, grams), ct);
        return nutrition is null ? NotFound() : Ok(nutrition);
    }

    /// <summary>Returns all diary entries for the authenticated user on the given date.</summary>
    [HttpGet("diary")]
    public async Task<IActionResult> GetDiaryEntries([FromQuery] DateOnly? date, CancellationToken ct)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var entries = await _sender.Send(new GetDiaryEntriesQuery(targetDate), ct);
        return Ok(entries);
    }

    /// <summary>Adds a food entry to the authenticated user's diary.</summary>
    [HttpPost("diary")]
    public async Task<IActionResult> AddDiaryEntry([FromBody] AddNutritionEntryRequest request, CancellationToken ct)
    {
        var command = new AddNutritionEntryCommand
        {
            MealType = request.MealType,
            FoodName = request.FoodName,
            Calories = request.Calories,
            ProteinGrams = request.ProteinGrams,
            CarbGrams = request.CarbGrams,
            FatGrams = request.FatGrams,
            ServingDescription = request.ServingDescription,
            Date = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var dto = await _sender.Send(command, ct);
        return Created($"/api/nutrition/diary", dto);
    }

    /// <summary>Removes a diary entry. Returns 204 on success.</summary>
    [HttpDelete("diary/{id:guid}")]
    public async Task<IActionResult> DeleteDiaryEntry(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteNutritionEntryCommand(id), ct);
        return NoContent();
    }
}

public sealed record AddNutritionEntryRequest(
    MealType MealType,
    string FoodName,
    double Calories,
    double ProteinGrams,
    double CarbGrams,
    double FatGrams,
    string? ServingDescription,
    DateOnly? Date);
