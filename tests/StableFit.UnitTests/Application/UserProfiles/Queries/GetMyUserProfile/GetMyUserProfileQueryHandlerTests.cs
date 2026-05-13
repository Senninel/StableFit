using FluentAssertions;
using NSubstitute;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;
using StableFit.Application.UserProfiles.Queries.GetMyUserProfile;
using StableFit.Domain.Entities;

namespace StableFit.UnitTests.Application.UserProfiles.Queries.GetMyUserProfile;

public class GetMyUserProfileQueryHandlerTests
{
    private readonly IUserProfileRepository _repositoryMock;
    private readonly ICurrentUserService _currentUserMock;
    private readonly GetMyUserProfileQueryHandler _handler;

    public GetMyUserProfileQueryHandlerTests()
    {
        _repositoryMock = Substitute.For<IUserProfileRepository>();
        _currentUserMock = Substitute.For<ICurrentUserService>();
        _handler = new GetMyUserProfileQueryHandler(_repositoryMock, _currentUserMock);
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserMock.UserId.Returns((string?)null);
        var query = new GetMyUserProfileQuery();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ProfileNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "user-123";
        _currentUserMock.UserId.Returns(userId);
        
        _repositoryMock.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((UserProfile?)null);

        var query = new GetMyUserProfileQuery();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ProfileExists_ReturnsUserProfileDto()
    {
        // Arrange
        var userId = "user-123";
        var id = Guid.NewGuid();
        _currentUserMock.UserId.Returns(userId);

        var profile = UserProfile.Create(userId, "testuser", "Test User", "test@example.com");
        profile.GetType().GetProperty(nameof(UserProfile.Id))!.SetValue(profile, id);

        _repositoryMock.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(profile);

        var query = new GetMyUserProfileQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.UserId.Should().Be(userId);
        result.Username.Should().Be("testuser");
    }
}
