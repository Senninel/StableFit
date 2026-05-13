using FluentValidation.TestHelper;
using StableFit.Application.Auth.Commands.Login;

namespace StableFit.UnitTests.Application.Auth.Commands.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new LoginCommand("test@example.com", "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("not-an-email")]
    public void Validate_InvalidEmail_ShouldHaveValidationError(string? invalidEmail)
    {
        var command = new LoginCommand(invalidEmail!, "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyPassword_ShouldHaveValidationError(string? emptyPassword)
    {
        var command = new LoginCommand("test@example.com", emptyPassword!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
