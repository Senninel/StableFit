using FluentValidation;
using StableFit.Domain.Enums;

namespace StableFit.Application.Matching.Commands.SubmitDecision;

public sealed class SubmitDecisionCommandValidator : AbstractValidator<SubmitDecisionCommand>
{
    public SubmitDecisionCommandValidator()
    {
        RuleFor(x => x.RecommendationId)
            .NotEmpty()
            .WithMessage("RecommendationId is required.");

        RuleFor(x => x.Decision)
            .IsInEnum()
            .WithMessage($"Decision must be one of: {string.Join(", ", Enum.GetNames<MatchDecisionType>())}.");
    }
}
