using MediatR;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;
using StableFit.Application.Nutrition.DTOs;
using StableFit.Domain.Entities;

namespace StableFit.Application.Nutrition.Commands.AddNutritionEntry;

public sealed class AddNutritionEntryCommandHandler : IRequestHandler<AddNutritionEntryCommand, NutritionEntryDto>
{
    private readonly INutritionEntryRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public AddNutritionEntryCommandHandler(
        INutritionEntryRepository repository,
        IApplicationDbContext dbContext,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<NutritionEntryDto> Handle(AddNutritionEntryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var entry = NutritionEntry.Create(
            userId,
            request.Date,
            request.MealType,
            request.FoodName,
            request.Calories,
            request.ProteinGrams,
            request.CarbGrams,
            request.FatGrams,
            request.ServingDescription);

        await _repository.AddAsync(entry, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(entry);
    }

    private static NutritionEntryDto MapToDto(NutritionEntry e) =>
        new(e.Id, e.MealType, e.FoodName, e.Calories, e.ProteinGrams, e.CarbGrams, e.FatGrams, e.ServingDescription, e.Date);
}
