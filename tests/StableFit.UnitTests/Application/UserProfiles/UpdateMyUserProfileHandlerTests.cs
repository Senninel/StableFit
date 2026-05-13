using FluentAssertions;
using NSubstitute;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;
using StableFit.Application.UserProfiles.Commands.UpdateMyUserProfile;
using StableFit.Domain.Entities;
using StableFit.Domain.Enums;

namespace StableFit.UnitTests.Application.UserProfiles;

public sealed class UpdateMyUserProfileHandlerTests : IDisposable
{
    private readonly IUserProfileRepository _repository;
    private readonly IApplicationDbContext  _dbContext;
    private readonly ICurrentUserService    _currentUser;
    private readonly UpdateMyUserProfileCommandHandler _sut;

    public UpdateMyUserProfileHandlerTests()
    {
        _repository  = Substitute.For<IUserProfileRepository>();
        _dbContext   = Substitute.For<IApplicationDbContext>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _sut         = new UpdateMyUserProfileCommandHandler(_repository, _dbContext, _currentUser);
    }

    public void Dispose() { /* NSubstitute substitutes are GC-managed */ }

    // -------------------------------------------------------------------------
    // Happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Given_ExistingProfile_When_Handle_Then_UpdatesAndSaves()
    {
        // Arrange
        const string userId = "identity-user-1";
        _currentUser.UserId.Returns(userId);

        var profile = UserProfile.Create(userId, "user1", "User One", "one@example.com");
        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(profile);

        var command = new UpdateMyUserProfileCommand(
            Bio: "Updated bio",
            Goal: FitnessGoal.MuscleGain,
            ScheduleDays: [DayOfWeek.Monday],
            AgeYears: 30,
            WeightKg: 85.0);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        profile.Bio.Should().Be("Updated bio");
        profile.Goal.Should().Be(FitnessGoal.MuscleGain);
        profile.ScheduleDays.Should().ContainSingle().Which.Should().Be(DayOfWeek.Monday);
        profile.AgeYears.Should().Be(30);
        profile.WeightKg.Should().Be(85.0);

        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------
    // Guards
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Given_NotAuthenticated_When_Handle_Then_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUser.UserId.Returns((string?)null);

        var command = new UpdateMyUserProfileCommand(null, null, null, null, null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
        await _repository.DidNotReceiveWithAnyArgs().GetByUserIdAsync(default!, default);
    }

    [Fact]
    public async Task Given_ProfileNotFound_When_Handle_Then_ThrowsNotFoundException()
    {
        // Arrange
        const string userId = "identity-user-1";
        _currentUser.UserId.Returns(userId);

        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((UserProfile?)null);

        var command = new UpdateMyUserProfileCommand(null, null, null, null, null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        await _dbContext.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }
}
