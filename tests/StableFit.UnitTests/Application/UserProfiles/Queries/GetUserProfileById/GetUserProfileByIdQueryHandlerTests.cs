using FluentAssertions;
using NSubstitute;
using StableFit.Application.Interfaces;
using StableFit.Application.UserProfiles.Queries.GetUserProfileById;
using StableFit.Domain.Entities;

namespace StableFit.UnitTests.Application.UserProfiles.Queries.GetUserProfileById;

public class GetUserProfileByIdQueryHandlerTests
{
    private readonly IUserProfileRepository _repositoryMock;
    private readonly GetUserProfileByIdQueryHandler _handler;

    public GetUserProfileByIdQueryHandlerTests()
    {
        _repositoryMock = Substitute.For<IUserProfileRepository>();
        _handler = new GetUserProfileByIdQueryHandler(_repositoryMock);
    }

    [Fact]
    public async Task Handle_ProfileExists_ReturnsUserProfileDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var profile = UserProfile.Create("user-1", "testuser", "Test User", "test@example.com");
        profile.GetType().GetProperty(nameof(UserProfile.Id))!.SetValue(profile, id);

        _repositoryMock.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(profile);

        var query = new GetUserProfileByIdQuery(id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.UserId.Should().Be("user-1");
        result.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task Handle_ProfileDoesNotExist_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((UserProfile?)null);

        var query = new GetUserProfileByIdQuery(id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
