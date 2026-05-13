using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using StableFit.Infrastructure.Identity;
using StableFit.Infrastructure.Services;

namespace StableFit.UnitTests.Infrastructure.Services;

public class IdentityServiceTests
{
    private readonly UserManager<AppUser> _userManagerMock;
    private readonly IdentityService _service;

    public IdentityServiceTests()
    {
        var store = Substitute.For<IUserStore<AppUser>>();
        _userManagerMock = Substitute.For<UserManager<AppUser>>(
            store, null!, null!, null!, null!, null!, null!, null!, null!);

        _service = new IdentityService(_userManagerMock);
    }

    [Fact]
    public async Task RegisterAsync_Success_ReturnsTrueAndUserId()
    {
        // Arrange
        _userManagerMock.CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);

        // Act
        var (succeeded, userId, errors) = await _service.RegisterAsync("test@example.com", "Password123!", "testuser");

        // Assert
        succeeded.Should().BeTrue();
        userId.Should().NotBeNull();
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_Failure_ReturnsFalseAndErrors()
    {
        // Arrange
        var identityErrors = new[]
        {
            new IdentityError { Description = "Email taken" },
            new IdentityError { Description = "Password too weak" }
        };
        _userManagerMock.CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(identityErrors));

        // Act
        var (succeeded, userId, errors) = await _service.RegisterAsync("test@example.com", "weak", "testuser");

        // Assert
        succeeded.Should().BeFalse();
        userId.Should().BeNull();
        errors.Should().HaveCount(2);
        errors.Should().Contain("Email taken");
        errors.Should().Contain("Password too weak");
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsFalse()
    {
        // Arrange
        _userManagerMock.FindByEmailAsync("missing@example.com")
            .Returns((AppUser?)null);

        // Act
        var (succeeded, userId, email, username) = await _service.LoginAsync("missing@example.com", "Password123!");

        // Assert
        succeeded.Should().BeFalse();
        userId.Should().BeNull();
        email.Should().BeNull();
        username.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFalse()
    {
        // Arrange
        var user = new AppUser { Id = "user-1", Email = "test@example.com", UserName = "testuser" };
        _userManagerMock.FindByEmailAsync(user.Email)
            .Returns(user);
        
        _userManagerMock.CheckPasswordAsync(user, "WrongPassword!")
            .Returns(false);

        // Act
        var (succeeded, userId, email, username) = await _service.LoginAsync(user.Email, "WrongPassword!");

        // Assert
        succeeded.Should().BeFalse();
        userId.Should().BeNull();
        email.Should().BeNull();
        username.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_UpdatesLastLoginAndReturnsTrue()
    {
        // Arrange
        var user = new AppUser { Id = "user-1", Email = "test@example.com", UserName = "testuser", LastLoginAt = DateTime.MinValue };
        _userManagerMock.FindByEmailAsync(user.Email)
            .Returns(user);
        
        _userManagerMock.CheckPasswordAsync(user, "Password123!")
            .Returns(true);

        _userManagerMock.UpdateAsync(user).Returns(IdentityResult.Success);

        // Act
        var (succeeded, userId, email, username) = await _service.LoginAsync(user.Email, "Password123!");

        // Assert
        succeeded.Should().BeTrue();
        userId.Should().Be(user.Id);
        email.Should().Be(user.Email);
        username.Should().Be(user.UserName);

        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        await _userManagerMock.Received(1).UpdateAsync(user);
    }
}
