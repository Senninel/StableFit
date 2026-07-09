using MediatR;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;
using StableFit.Application.Nutrition.DTOs;
using StableFit.Domain.Entities;

namespace StableFit.Application.Nutrition.Queries.GetDiaryEntries;

public sealed class GetDiaryEntriesQueryHandler : IRequestHandler<GetDiaryEntriesQuery, IReadOnlyList<NutritionEntryDto>>
{
    private readonly INutritionEntryRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetDiaryEntriesQueryHandler(INutritionEntryRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<NutritionEntryDto>> Handle(GetDiaryEntriesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var entries = await _repository.GetByUserIdAndDateAsync(userId, request.Date, cancellationToken);

        return entries
            .Select(static e => new NutritionEntryDto(
                e.Id, e.MealType, e.FoodName, e.Calories, e.ProteinGrams, e.CarbGrams, e.FatGrams, e.ServingDescription, e.Date))
            .ToList()
            .AsReadOnly();
    }
}
