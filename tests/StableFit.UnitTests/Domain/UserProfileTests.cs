using FluentAssertions;
using StableFit.Domain.Entities;
using StableFit.Domain.Enums;

namespace StableFit.UnitTests.Domain;

public sealed class UserProfileTests
{
    // -------------------------------------------------------------------------
    // UserProfile.Create — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public void Given_ValidArgs_When_Create_Then_ReturnsProfileWithCorrectProperties()
    {
        // Arrange
        const string userId   = "user-123";
        const string username = "ironmike";
        const string name     = "Mike Tyson";
        const string email    = "mike@example.com";

        // Act
        var profile = UserProfile.Create(userId, username, name, email);

        // Assert
        profile.UserId.Should().Be(userId);
        profile.Username.Should().Be(username);
        profile.Name.Should().Be(name);
        profile.Email.Should().Be(email);
        profile.Id.Should().NotBeEmpty();
        profile.ScheduleDays.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidArgs_When_Create_Then_EmailIsNormalisedToLowercase()
    {
        // Arrange & Act
        var profile = UserProfile.Create("u1", "user1", "User One", "UPPER@EXAMPLE.COM");

        // Assert
        profile.Email.Should().Be("upper@example.com");
    }

    [Fact]
    public void Given_ValidArgs_When_Create_Then_IdIsUnique()
    {
        // Arrange & Act
        var p1 = UserProfile.Create("u1", "user1", "User One", "one@example.com");
        var p2 = UserProfile.Create("u2", "user2", "User Two", "two@example.com");

        // Assert
        p1.Id.Should().NotBe(p2.Id);
    }

    // -------------------------------------------------------------------------
    // UserProfile.Create — guard clauses
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_EmptyUserId_When_Create_Then_ThrowsArgumentException(string userId)
    {
        // Arrange & Act
        var act = () => UserProfile.Create(userId, "user", "Name", "e@x.com");

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("userId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_EmptyUsername_When_Create_Then_ThrowsArgumentException(string username)
    {
        // Arrange & Act
        var act = () => UserProfile.Create("u1", username, "Name", "e@x.com");

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("username");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_EmptyName_When_Create_Then_ThrowsArgumentException(string name)
    {
        // Arrange & Act
        var act = () => UserProfile.Create("u1", "user", name, "e@x.com");

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_EmptyEmail_When_Create_Then_ThrowsArgumentException(string email)
    {
        // Arrange & Act
        var act = () => UserProfile.Create("u1", "user", "Name", email);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("email");
    }

    // -------------------------------------------------------------------------
    // UserProfile.Update — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public void Given_ValidValues_When_Update_Then_PropertiesAreChanged()
    {
        // Arrange
        var profile = UserProfile.Create("u1", "user1", "User One", "one@example.com");

        // Act
        profile.Update(
            bio: "I love lifting",
            goal: FitnessGoal.MuscleGain,
            scheduleDays: [DayOfWeek.Monday, DayOfWeek.Wednesday],
            ageYears: 28,
            weightKg: 80.5);

        // Assert
        profile.Bio.Should().Be("I love lifting");
        profile.Goal.Should().Be(FitnessGoal.MuscleGain);
        profile.ScheduleDays.Should().BeEquivalentTo([DayOfWeek.Monday, DayOfWeek.Wednesday]);
        profile.AgeYears.Should().Be(28);
        profile.WeightKg.Should().Be(80.5);
    }

    [Fact]
    public void Given_NullValues_When_Update_Then_PropertiesRemainUnchanged()
    {
        // Arrange
        var profile = UserProfile.Create("u1", "user1", "User One", "one@example.com");
        profile.Update(bio: "original bio", goal: FitnessGoal.WeightLoss,
            scheduleDays: null, ageYears: null, weightKg: null);

        // Act
        profile.Update(bio: null, goal: null, scheduleDays: null, ageYears: null, weightKg: null);

        // Assert
        profile.Bio.Should().Be("original bio");
        profile.Goal.Should().Be(FitnessGoal.WeightLoss);
    }

    // -------------------------------------------------------------------------
    // UserProfile.Update — guard clauses
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Given_NegativeAge_When_Update_Then_ThrowsArgumentException(int age)
    {
        // Arrange
        var profile = UserProfile.Create("u1", "user1", "User One", "one@example.com");

        // Act
        var act = () => profile.Update(null, null, null, ageYears: age, weightKg: null);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("ageYears");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-10.5)]
    public void Given_ZeroOrNegativeWeight_When_Update_Then_ThrowsArgumentException(double weight)
    {
        // Arrange
        var profile = UserProfile.Create("u1", "user1", "User One", "one@example.com");

        // Act
        var act = () => profile.Update(null, null, null, ageYears: null, weightKg: weight);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("weightKg");
    }
}
