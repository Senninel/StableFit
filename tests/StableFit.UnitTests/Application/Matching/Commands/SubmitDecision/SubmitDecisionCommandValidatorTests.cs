using FluentValidation.TestHelper;
using StableFit.Application.Matching.Commands.SubmitDecision;
using StableFit.Domain.Enums;

namespace StableFit.UnitTests.Application.Matching.Commands.SubmitDecision;

public class SubmitDecisionCommandValidatorTests
{
    private readonly SubmitDecisionCommandValidator _validator;

    public SubmitDecisionCommandValidatorTests()
    {
        _validator = new SubmitDecisionCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new SubmitDecisionCommand(Guid.NewGuid(), MatchDecisionType.Accepted);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyRecommendationId_ShouldHaveValidationError()
    {
        var command = new SubmitDecisionCommand(Guid.Empty, MatchDecisionType.Accepted);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RecommendationId)
              .WithErrorMessage("RecommendationId is required.");
    }

    [Fact]
    public void Validate_InvalidDecisionEnum_ShouldHaveValidationError()
    {
        var command = new SubmitDecisionCommand(Guid.NewGuid(), (MatchDecisionType)999);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Decision)
              .WithErrorMessage($"Decision must be one of: {string.Join(", ", Enum.GetNames<MatchDecisionType>())}.");
    }
}
