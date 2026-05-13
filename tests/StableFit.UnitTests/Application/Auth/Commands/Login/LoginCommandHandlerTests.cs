using FluentAssertions;
using NSubstitute;
using StableFit.Application.Auth.Commands.Login;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;

namespace StableFit.UnitTests.Application.Auth.Commands.Login;

public class LoginCommandHandlerTests
{
    private readonly IIdentityService _identityServiceMock;
    private readonly ITokenService _tokenServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _identityServiceMock = Substitute.For<IIdentityService>();
        _tokenServiceMock = Substitute.For<ITokenService>();
        _handler = new LoginCommandHandler(_identityServiceMock, _tokenServiceMock);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResultDto()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");
        var userId = "user-123";
        var username = "testuser";
        var expectedToken = "jwt-token-string";

        _identityServiceMock.LoginAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns((true, userId, command.Email, username));

        _tokenServiceMock.CreateToken(userId, command.Email, username)
            .Returns(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Email.Should().Be(command.Email);
        result.Username.Should().Be(username);
        result.AccessToken.Should().Be(expectedToken);

        await _identityServiceMock.Received(1).LoginAsync(command.Email, command.Password, Arg.Any<CancellationToken>());
        _tokenServiceMock.Received(1).CreateToken(userId, command.Email, username);
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ThrowsUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "WrongPassword!");

        _identityServiceMock.LoginAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns((false, null, null, null));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");

        _tokenServiceMock.DidNotReceiveWithAnyArgs().CreateToken(default!, default!, default!);
    }

    [Fact]
    public async Task Handle_SucceededButNullUserId_ThrowsUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");

        _identityServiceMock.LoginAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns((true, null, command.Email, "username"));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
