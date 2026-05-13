using FluentValidation.TestHelper;
using StableFit.Application.UserProfiles.Commands.CreateUserProfile;

namespace StableFit.UnitTests.Application.UserProfiles.Commands.CreateUserProfile;

public class CreateUserProfileCommandValidatorTests
{
    private readonly CreateUserProfileCommandValidator _validator;

    public CreateUserProfileCommandValidatorTests()
    {
        _validator = new CreateUserProfileCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new CreateUserProfileCommand("user-123", "testuser", "Test User", "test@example.com");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyUsername_ShouldHaveValidationError(string? emptyUsername)
    {
        var command = new CreateUserProfileCommand("user-123", emptyUsername!, "Test User", "test@example.com");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_UsernameTooLong_ShouldHaveValidationError()
    {
        var longUsername = new string('a', 51);
        var command = new CreateUserProfileCommand("user-123", longUsername, "Test User", "test@example.com");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyName_ShouldHaveValidationError(string? emptyName)
    {
        var command = new CreateUserProfileCommand("user-123", "testuser", emptyName!, "test@example.com");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        var longName = new string('a', 201);
        var command = new CreateUserProfileCommand("user-123", "testuser", longName, "test@example.com");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("not-an-email")]
    public void Validate_InvalidEmail_ShouldHaveValidationError(string? invalidEmail)
    {
        var command = new CreateUserProfileCommand("user-123", "testuser", "Test User", invalidEmail!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldHaveValidationError()
    {
        var longEmail = new string('a', 311) + "@example.com"; // 321 chars
        var command = new CreateUserProfileCommand("user-123", "testuser", "Test User", longEmail);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
