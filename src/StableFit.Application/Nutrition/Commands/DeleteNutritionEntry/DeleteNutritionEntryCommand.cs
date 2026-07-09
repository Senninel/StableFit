using MediatR;

namespace StableFit.Application.Nutrition.Commands.DeleteNutritionEntry;

public sealed record DeleteNutritionEntryCommand(Guid EntryId) : IRequest;
