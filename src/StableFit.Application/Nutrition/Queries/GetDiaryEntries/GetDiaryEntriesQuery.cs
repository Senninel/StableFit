using MediatR;
using StableFit.Application.Nutrition.DTOs;

namespace StableFit.Application.Nutrition.Queries.GetDiaryEntries;

public sealed record GetDiaryEntriesQuery(DateOnly Date) : IRequest<IReadOnlyList<NutritionEntryDto>>;
