using FluentValidation.TestHelper;
using StableFit.Application.UserProfiles.Commands.UpdateMyUserProfile;
using StableFit.Domain.Enums;

namespace StableFit.UnitTests.Application.UserProfiles.Commands.UpdateMyUserProfile;

public class UpdateMyUserProfileCommandValidatorTests
{
    private readonly UpdateMyUserProfileCommandValidator _validator;

    public UpdateMyUserProfileCommandValidatorTests()
    {
        _validator = new UpdateMyUserProfileCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new UpdateMyUserProfileCommand("A bit about me", FitnessGoal.WeightLoss, new List<DayOfWeek> { DayOfWeek.Monday }, 25, 75.5);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NullValues_ShouldNotHaveAnyErrors()
    {
        // All fields are optional (nullable)
        var command = new UpdateMyUserProfileCommand(null, null, null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_BioTooLong_ShouldHaveValidationError()
    {
        var longBio = new string('a', 501);
        var command = new UpdateMyUserProfileCommand(longBio, null, null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Bio);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(121)]
    public void Validate_AgeOutOfrange_ShouldHaveValidationError(int invalidAge)
    {
        var command = new UpdateMyUserProfileCommand(null, null, null, invalidAge, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AgeYears);
    }

    [Theory]
    [InlineData(19.9)]
    [InlineData(500.1)]
    public void Validate_WeightOutOfRange_ShouldHaveValidationError(double invalidWeight)
    {
        var command = new UpdateMyUserProfileCommand(null, null, null, null, invalidWeight);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.WeightKg);
    }
}
