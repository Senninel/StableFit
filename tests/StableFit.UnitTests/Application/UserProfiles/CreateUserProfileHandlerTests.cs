using FluentAssertions;
using NSubstitute;
using StableFit.Application.Interfaces;
using StableFit.Application.UserProfiles.Commands.CreateUserProfile;
using StableFit.Application.UserProfiles.DTOs;
using StableFit.Domain.Entities;

namespace StableFit.UnitTests.Application.UserProfiles;

public sealed class CreateUserProfileHandlerTests : IDisposable
{
    private readonly IUserProfileRepository _repository;
    private readonly IApplicationDbContext  _dbContext;
    private readonly CreateUserProfileCommandHandler _sut;

    public CreateUserProfileHandlerTests()
    {
        _repository = Substitute.For<IUserProfileRepository>();
        _dbContext  = Substitute.For<IApplicationDbContext>();
        _sut        = new CreateUserProfileCommandHandler(_repository, _dbContext);
    }

    public void Dispose() { /* NSubstitute substitutes are GC-managed */ }

    // -------------------------------------------------------------------------
    // Happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Given_ValidCommand_When_Handle_Then_ReturnsProfileDtoWithCorrectValues()
    {
        // Arrange
        var command = new CreateUserProfileCommand(
            UserId:   "identity-user-1",
            Username: "athlete99",
            Name:     "Jane Doe",
            Email:    "jane@example.com");

        _repository.GetByUsernameAsync(command.Username, Arg.Any<CancellationToken>())
            .Returns((UserProfile?)null);
        _repository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((UserProfile?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UserProfileDto>();
        result.UserId.Should().Be(command.UserId);
        result.Username.Should().Be(command.Username);
        result.Name.Should().Be(command.Name);
        result.Email.Should().Be(command.Email.ToLowerInvariant());
    }

    [Fact]
    public async Task Given_ValidCommand_When_Handle_Then_AddsProfileAndSavesChanges()
    {
        // Arrange
        var command = new CreateUserProfileCommand("uid-1", "user1", "User One", "one@example.com");

        _repository.GetByUsernameAsync(command.Username, Arg.Any<CancellationToken>())
            .Returns((UserProfile?)null);
        _repository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((UserProfile?)null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1)
            .AddAsync(Arg.Is<UserProfile>(p => p.UserId == command.UserId), Arg.Any<CancellationToken>());
        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------
    // Duplicate username
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Given_DuplicateUsername_When_Handle_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var command  = new CreateUserProfileCommand("uid-1", "takenuser", "User One", "one@example.com");
        var existing = UserProfile.Create("other-uid", "takenuser", "Other User", "other@example.com");

        _repository.GetByUsernameAsync(command.Username, Arg.Any<CancellationToken>())
            .Returns(existing);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Username*already in use*");
        await _repository.DidNotReceive().AddAsync(Arg.Any<UserProfile>(), Arg.Any<CancellationToken>());
        await _dbContext.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------
    // Duplicate email
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Given_DuplicateEmail_When_Handle_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var command  = new CreateUserProfileCommand("uid-1", "newuser", "User One", "taken@example.com");
        var existing = UserProfile.Create("other-uid", "otheruser", "Other User", "taken@example.com");

        _repository.GetByUsernameAsync(command.Username, Arg.Any<CancellationToken>())
            .Returns((UserProfile?)null);
        _repository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(existing);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Email*already in use*");
        await _dbContext.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
