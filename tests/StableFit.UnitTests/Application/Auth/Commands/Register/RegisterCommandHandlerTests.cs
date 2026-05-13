using FluentAssertions;
using NSubstitute;
using StableFit.Application.Auth.Commands.Register;
using StableFit.Application.Interfaces;

namespace StableFit.UnitTests.Application.Auth.Commands.Register;

public class RegisterCommandHandlerTests
{
    private readonly IIdentityService _identityServiceMock;
    private readonly ITokenService _tokenServiceMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _identityServiceMock = Substitute.For<IIdentityService>();
        _tokenServiceMock = Substitute.For<ITokenService>();
        _handler = new RegisterCommandHandler(_identityServiceMock, _tokenServiceMock);
    }

    [Fact]
    public async Task Handle_ValidRegistration_ReturnsAuthResultDto()
    {
        // Arrange
        var command = new RegisterCommand("newuser@example.com", "Password123!", "newuser");
        var userId = "user-456";
        var expectedToken = "jwt-register-token";

        _identityServiceMock.RegisterAsync(command.Email, command.Password, command.Username, Arg.Any<CancellationToken>())
            .Returns((true, userId, Array.Empty<string>()));

        _tokenServiceMock.CreateToken(userId, command.Email, command.Username)
            .Returns(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Email.Should().Be(command.Email);
        result.Username.Should().Be(command.Username);
        result.AccessToken.Should().Be(expectedToken);

        await _identityServiceMock.Received(1).RegisterAsync(command.Email, command.Password, command.Username, Arg.Any<CancellationToken>());
        _tokenServiceMock.Received(1).CreateToken(userId, command.Email, command.Username);
    }

    [Fact]
    public async Task Handle_RegistrationFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RegisterCommand("existing@example.com", "Password123!", "existinguser");
        var errors = new List<string> { "Email already in use", "Username taken" };

        _identityServiceMock.RegisterAsync(command.Email, command.Password, command.Username, Arg.Any<CancellationToken>())
            .Returns((false, null, errors));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email already in use; Username taken");

        _tokenServiceMock.DidNotReceiveWithAnyArgs().CreateToken(default!, default!, default!);
    }

    [Fact]
    public async Task Handle_SucceededButNullUserId_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RegisterCommand("newuser@example.com", "Password123!", "newuser");
        var errors = new List<string> { "An unexpected error occurred" };

        _identityServiceMock.RegisterAsync(command.Email, command.Password, command.Username, Arg.Any<CancellationToken>())
            .Returns((true, null, errors));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("An unexpected error occurred");
    }
}
