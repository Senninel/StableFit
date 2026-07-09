using FluentValidation;

namespace StableFit.Application.Nutrition.Commands.AddNutritionEntry;

public sealed class AddNutritionEntryCommandValidator : AbstractValidator<AddNutritionEntryCommand>
{
    public AddNutritionEntryCommandValidator()
    {
        RuleFor(x => x.FoodName)
            .NotEmpty().WithMessage("Food name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Calories)
            .InclusiveBetween(0, 10000).WithMessage("Calories must be between 0 and 10,000.");

        RuleFor(x => x.ProteinGrams)
            .GreaterThanOrEqualTo(0).WithMessage("Protein cannot be negative.");

        RuleFor(x => x.CarbGrams)
            .GreaterThanOrEqualTo(0).WithMessage("Carbohydrates cannot be negative.");

        RuleFor(x => x.FatGrams)
            .GreaterThanOrEqualTo(0).WithMessage("Fat cannot be negative.");

        RuleFor(x => x.ServingDescription)
            .MaximumLength(50)
            .When(x => x.ServingDescription is not null);
    }
}
