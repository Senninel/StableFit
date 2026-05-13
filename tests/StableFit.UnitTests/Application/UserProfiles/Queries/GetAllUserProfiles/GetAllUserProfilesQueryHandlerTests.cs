using FluentAssertions;
using NSubstitute;
using StableFit.Application.Interfaces;
using StableFit.Application.UserProfiles.Queries.GetAllUserProfiles;
using StableFit.Domain.Entities;

namespace StableFit.UnitTests.Application.UserProfiles.Queries.GetAllUserProfiles;

public class GetAllUserProfilesQueryHandlerTests
{
    private readonly IUserProfileRepository _repositoryMock;
    private readonly GetAllUserProfilesQueryHandler _handler;

    public GetAllUserProfilesQueryHandlerTests()
    {
        _repositoryMock = Substitute.For<IUserProfileRepository>();
        _handler = new GetAllUserProfilesQueryHandler(_repositoryMock);
    }

    [Fact]
    public async Task Handle_ProfilesExist_ReturnsListOfDtos()
    {
        // Arrange
        var profile1 = UserProfile.Create("user-1", "user1", "User One", "user1@example.com");
        var profile2 = UserProfile.Create("user-2", "user2", "User Two", "user2@example.com");

        _repositoryMock.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<UserProfile> { profile1, profile2 });

        var query = new GetAllUserProfilesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Username.Should().Be("user1");
        result[1].Username.Should().Be("user2");
    }

    [Fact]
    public async Task Handle_NoProfiles_ReturnsEmptyList()
    {
        // Arrange
        _repositoryMock.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<UserProfile>());

        var query = new GetAllUserProfilesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
