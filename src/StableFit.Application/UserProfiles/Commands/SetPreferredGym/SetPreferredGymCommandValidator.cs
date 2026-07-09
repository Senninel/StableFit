using FluentValidation;

namespace StableFit.Application.UserProfiles.Commands.SetPreferredGym;

public sealed class SetPreferredGymCommandValidator : AbstractValidator<SetPreferredGymCommand>
{
    public SetPreferredGymCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Gym name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
    }
}
