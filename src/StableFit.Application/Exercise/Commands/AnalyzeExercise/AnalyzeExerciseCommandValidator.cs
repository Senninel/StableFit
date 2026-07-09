using FluentValidation;

namespace StableFit.Application.Exercise.Commands.AnalyzeExercise;

public sealed class AnalyzeExerciseCommandValidator : AbstractValidator<AnalyzeExerciseCommand>
{
    private static readonly HashSet<string> SupportedExercises =
    [
        "squat", "pushup", "plank", "bicep_curl", "lunge", "pullup"
    ];

    public AnalyzeExerciseCommandValidator()
    {
        RuleFor(x => x.VideoBytes)
            .NotEmpty()
            .WithMessage("Video file is required.");

        RuleFor(x => x.ExerciseType)
            .NotEmpty()
            .Must(SupportedExercises.Contains)
            .WithMessage($"Exercise type must be one of: {string.Join(", ", SupportedExercises)}.");
    }
}
