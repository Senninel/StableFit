using FluentAssertions;
using StableFit.Application.Matching.Services;
using StableFit.Domain.Entities;
using StableFit.Domain.Enums;

namespace StableFit.UnitTests.Application.Matching;

/// <summary>
/// Tests for <see cref="DefaultMatchScoringService"/>.
/// All scoring dimensions are exercised via the public Score() entry point.
/// Pure unit tests — no mocks required.
/// </summary>
public sealed class DefaultMatchScoringServiceTests
{
    private readonly DefaultMatchScoringService _sut = new();

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static UserProfile BuildProfile(
        string userId      = "u1",
        string username    = "user1",
        FitnessGoal goal   = FitnessGoal.MuscleGain,
        int? age           = null,
        double? weight     = null,
        params DayOfWeek[] days)
    {
        var profile = UserProfile.Create(userId, username, username, $"{username}@x.com");
        profile.Update(null, goal, days.Length > 0 ? [.. days] : null, age, weight);
        return profile;
    }

    // -------------------------------------------------------------------------
    // Self-match guard
    // -------------------------------------------------------------------------

    [Fact]
    public void Given_SameProfile_When_Score_Then_ReturnsZero()
    {
        // Arrange
        var p = BuildProfile("u1", "user1");

        // Act
        var result = _sut.Score(p, p);

        // Assert
        result.Should().Be(0);
    }

    // -------------------------------------------------------------------------
    // Goal scoring (+50)
    // -------------------------------------------------------------------------

    [Fact]
    public void Given_SameGoal_When_Score_Then_GoalBonusIs50()
    {
        // Arrange
        var a = BuildProfile("u1", "user1", goal: FitnessGoal.MuscleGain);
        var b = BuildProfile("u2", "user2", goal: FitnessGoal.MuscleGain);

        // Act
        var result = _sut.Score(a, b);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(50);
    }

    [Fact]
    public void Given_DifferentGoal_When_Score_Then_NoGoalBonus()
    {
        // Arrange — only goals differ, everything else absent
        var a = BuildProfile("u1", "user1", goal: FitnessGoal.MuscleGain);
        var b = BuildProfile("u2", "user2", goal: FitnessGoal.WeightLoss);

        // Act
        var result = _sut.Score(a, b);

        // Assert
        result.Should().Be(0);
    }

    // -------------------------------------------------------------------------
    // Schedule overlap scoring (0..40, capped)
    // -------------------------------------------------------------------------

    [Fact]
    public void Given_FullWeekOverlap_When_Score_Then_ScheduleBonusCapsAt40()
    {
        // Arrange
        var days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                           DayOfWeek.Thursday, DayOfWeek.Friday };
        var a = BuildProfile("u1", "user1", goal: FitnessGoal.WeightLoss, days: days);
        var b = BuildProfile("u2", "user2", goal: FitnessGoal.WeightLoss, days: days);

        // Act
        var result = _sut.Score(a, b);

        // Assert — goal(50) + schedule(40) = 90
        result.Should().Be(90);
    }

    [Fact]
    public void Given_TwoDayOverlap_When_Score_Then_ScheduleBonusIs20()
    {
        // Arrange
        var a = BuildProfile("u1", "user1", goal: FitnessGoal.WeightLoss,
            days: [DayOfWeek.Monday, DayOfWeek.Wednesday]);
        var b = BuildProfile("u2", "user2", goal: FitnessGoal.WeightLoss,
            days: [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday]);

        // Act
        var result = _sut.Score(a, b);

        // Assert — goal(50) + schedule(20) = 70
        result.Should().Be(70);
    }

    [Fact]
    public void Given_NoScheduleDays_When_Score_Then_NoScheduleBonus()
    {
        // Arrange — no days set on either side
        var a = BuildProfile("u1", "user1", goal: FitnessGoal.WeightLoss);
        var b = BuildProfile("u2", "user2", goal: FitnessGoal.WeightLoss);

        // Act
        var result = _sut.Score(a, b);

        // Assert — only goal bonus
        result.Should().Be(50);
    }

    // -------------------------------------------------------------------------
    // Age closeness scoring
    // -------------------------------------------------------------------------

    [Fact]
    public void Given_AgeDiffWithin2_When_Score_Then_AgeBonusIs15()
    {
        // Arrange
        var a = BuildProfile("u1", "user1", age: 25, goal: FitnessGoal.WeightLoss);
        var b = BuildProfile("u2", "user2", age: 27, goal: FitnessGoal.MuscleGain);

        // Act
        var result = _sut.Score(a, b);

        // Assert — no goal match, no schedule → only age
        result.Should().Be(15);
    }

    [Fact]
    public void Given_AgeDiffWithin5_When_Score_Then_AgeBonusIs10()
    {
        // Arrange
        var a = BuildProfile("u1", "user1", age: 25, goal: FitnessGoal.WeightLoss);
        var b = BuildProfile("u2", "user2", age: 29, goal: FitnessGoal.MuscleGain);

        // Act
        var result = _sut.Score(a, b);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public void Given_AgeDiffWithin10_When_Score_Then_AgeBonusIs5()
    {
        // Arrange
        var a = BuildProfile("u1", "user1", age: 25, goal: FitnessGoal.WeightLoss);
        var b = BuildProfile("u2", "user2", age: 33, goal: FitnessGoal.MuscleGain);

        // Act
        var result = _sut.Score(a, b);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void Given_AgeDiffOver10_When_Score_Then_NoAgeBonus()
    {
        // Arrange
        var a = BuildProfile("u1", "user1", age: 20, goal: FitnessGoal.WeightLoss);
        var b = BuildProfile("u2", "user2", age: 35, goal: FitnessGoal.MuscleGain);

        // Act
        var result = _sut.Score(a, b);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Given_AgeNotSet_When_Score_Then_NoAgeBonus()
    {
        // Arrange — age left null
        var a = BuildProfile("u1", "user1", goal: FitnessGoal.WeightLoss);
        var b = BuildProfile("u2", "user2", goal: FitnessGoal.MuscleGain);

        // Act
        var result = _sut.Score(a, b);

        // Assert
        result.Should().Be(0);
    }

    // -------------------------------------------------------------------------
    // Weight closeness scoring
    // -------------------------------------------------------------------------

    [Fact]
    public void Given_WeightDiffWithin5_When_Score_Then_WeightBonusIs10()
    {
        // Arrange
        var a = BuildProfile("u1", "user1", weight: 80.0, goal: FitnessGoal.WeightLoss);
        var b = BuildProfile("u2", "user2", weight: 83.0, goal: FitnessGoal.MuscleGain);

        // Act
        var result = _sut.Score(a, b);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public void Given_WeightDiffWithin10_When_Score_Then_WeightBonusIs5()
    {
        // Arrange
        var a = BuildProfile("u1", "user1", weight: 70.0, goal: FitnessGoal.WeightLoss);
        var b = BuildProfile("u2", "user2", weight: 78.0, goal: FitnessGoal.MuscleGain);

        // Act
        var result = _sut.Score(a, b);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void Given_WeightDiffOver10_When_Score_Then_NoWeightBonus()
    {
        // Arrange
        var a = BuildProfile("u1", "user1", weight: 60.0, goal: FitnessGoal.WeightLoss);
        var b = BuildProfile("u2", "user2", weight: 90.0, goal: FitnessGoal.MuscleGain);

        // Act
        var result = _sut.Score(a, b);

        // Assert
        result.Should().Be(0);
    }

    // -------------------------------------------------------------------------
    // Perfect match (max score integration)
    // -------------------------------------------------------------------------

    [Fact]
    public void Given_PerfectMatchProfiles_When_Score_Then_ReturnsMaximumScore()
    {
        // Arrange — same goal, 5+ overlapping days, age diff ≤2, weight diff ≤5
        var days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                           DayOfWeek.Thursday, DayOfWeek.Friday };

        var a = BuildProfile("u1", "user1", goal: FitnessGoal.MuscleGain,
            age: 25, weight: 80.0, days: days);
        var b = BuildProfile("u2", "user2", goal: FitnessGoal.MuscleGain,
            age: 26, weight: 82.0, days: days);

        // Act
        var result = _sut.Score(a, b);

        // Assert — goal(50) + schedule(40) + age(15) + weight(10) = 115
        result.Should().Be(115);
    }
}
