using FluentValidation.TestHelper;
using StableFit.Application.Auth.Commands.Register;

namespace StableFit.UnitTests.Application.Auth.Commands.Register;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new RegisterCommand("test@example.com", "Password123!", "testuser");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("not-an-email")]
    public void Validate_InvalidEmail_ShouldHaveValidationError(string? invalidEmail)
    {
        var command = new RegisterCommand(invalidEmail!, "Password123!", "testuser");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldHaveValidationError()
    {
        var longEmail = new string('a', 311) + "@example.com"; // 321 chars
        var command = new RegisterCommand(longEmail, "Password123!", "testuser");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("short")] // < 8 chars
    public void Validate_InvalidPassword_ShouldHaveValidationError(string? invalidPassword)
    {
        var command = new RegisterCommand("test@example.com", invalidPassword!, "testuser");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_PasswordTooLong_ShouldHaveValidationError()
    {
        var longPassword = new string('a', 101);
        var command = new RegisterCommand("test@example.com", longPassword, "testuser");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("ab")] // < 3 chars
    public void Validate_InvalidUsername_ShouldHaveValidationError(string? invalidUsername)
    {
        var command = new RegisterCommand("test@example.com", "Password123!", invalidUsername!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_UsernameTooLong_ShouldHaveValidationError()
    {
        var longUsername = new string('a', 51);
        var command = new RegisterCommand("test@example.com", "Password123!", longUsername);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }
}
