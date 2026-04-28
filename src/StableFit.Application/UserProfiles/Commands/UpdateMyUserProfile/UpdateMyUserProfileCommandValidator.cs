using FluentValidation;

namespace StableFit.Application.UserProfiles.Commands.UpdateMyUserProfile;

public sealed class UpdateMyUserProfileCommandValidator : AbstractValidator<UpdateMyUserProfileCommand>
{
    public UpdateMyUserProfileCommandValidator()
    {
        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .When(x => x.Bio is not null);

        RuleFor(x => x.AgeYears)
            .InclusiveBetween(10, 120)
            .When(x => x.AgeYears is not null);

        RuleFor(x => x.WeightKg)
            .InclusiveBetween(20.0, 500.0)
            .When(x => x.WeightKg is not null);
    }
}
