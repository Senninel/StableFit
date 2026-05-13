using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using StableFit.Application.Matching.Interfaces;
using StableFit.Domain.Entities;
using StableFit.Domain.Enums;
using StableFit.Infrastructure.Identity;
using StableFit.Infrastructure.Persistence;
using StableFit.Infrastructure.Persistence.Entities;
using StableFit.Infrastructure.Services.Matching;

namespace StableFit.UnitTests.Infrastructure.Services.Matching;

public class MatchingRunBuilderTests
{
    private readonly IMatchScoringService _scoringMock;
    private readonly MatchingRunOptions _defaultOptions = new()
    {
        TopK = 3,
        ActiveWithinDays = 14,
        RunTtlHours = 24
    };

    public MatchingRunBuilderTests()
    {
        _scoringMock = Substitute.For<IMatchScoringService>();
        // Default: return a fixed score so ordering is deterministic
        _scoringMock.Score(Arg.Any<UserProfile>(), Arg.Any<UserProfile>()).Returns(50);
    }

    private StableFitDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<StableFitDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new StableFitDbContext(options);
    }

    private MatchingRunBuilder CreateBuilder(StableFitDbContext db, MatchingRunOptions? options = null)
        => new(db, _scoringMock, Options.Create(options ?? _defaultOptions));

    // ──────────────────────────────────────────────────────────
    // EnsureRunAndMaterializeRecommendationsAsync
    // ──────────────────────────────────────────────────────────

    [Fact]
    public async Task EnsureRun_ExistingValidRun_ReusesItWithoutCreatingNew()
    {
        await using var db = GetInMemoryDbContext();

        // Seed a still-valid run
        var validRun = MatchRun.Create(DateTime.UtcNow, TimeSpan.FromDays(1), 5);
        db.MatchRuns.Add(validRun);
        await db.SaveChangesAsync();

        var builder = CreateBuilder(db);
        var runId = await builder.EnsureRunAndMaterializeRecommendationsAsync(CancellationToken.None);

        runId.Should().Be(validRun.Id);
        db.MatchRuns.Count().Should().Be(1); // no new run created
    }

    [Fact]
    public async Task EnsureRun_NoValidRun_CreatesNewRun()
    {
        await using var db = GetInMemoryDbContext();
        await SeedEligibleUsersAsync(db, count: 2);

        var builder = CreateBuilder(db);
        var runId = await builder.EnsureRunAndMaterializeRecommendationsAsync(CancellationToken.None);

        runId.Should().NotBeEmpty();
        var run = await db.MatchRuns.FindAsync(runId);
        run.Should().NotBeNull();
        run!.ExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task EnsureRun_WithEnoughEligibleProfiles_MaterializesRecommendations()
    {
        await using var db = GetInMemoryDbContext();
        // 3 eligible users → each gets recommendations for the other 2 (capped at TopK=3)
        await SeedEligibleUsersAsync(db, count: 3);

        var builder = CreateBuilder(db);
        await builder.EnsureRunAndMaterializeRecommendationsAsync(CancellationToken.None);

        // Each of 3 users gets min(2, TopK) = 2 recommendations → 6 total
        var recs = await db.MatchRecommendations.ToListAsync();
        recs.Should().HaveCount(6);
    }

    [Fact]
    public async Task EnsureRun_FewerThan2EligibleProfiles_CreatesRunWithNoRecommendations()
    {
        await using var db = GetInMemoryDbContext();
        await SeedEligibleUsersAsync(db, count: 1);

        var builder = CreateBuilder(db);
        await builder.EnsureRunAndMaterializeRecommendationsAsync(CancellationToken.None);

        var recs = await db.MatchRecommendations.ToListAsync();
        recs.Should().BeEmpty(); // Not enough users to pair
    }

    [Fact]
    public async Task EnsureRun_ExcludesUsersWithActiveMatch()
    {
        await using var db = GetInMemoryDbContext();

        // 3 eligible users seeded, but 1 will be in an active match
        var users = await SeedEligibleUsersAsync(db, count: 3);
        var matchedUserId = users[0].UserId;

        // Give user[0] an active match
        db.Matches.Add(Match.CreateActive(matchedUserId, "some-external-partner", DateTime.UtcNow));
        await db.SaveChangesAsync();

        var builder = CreateBuilder(db);
        await builder.EnsureRunAndMaterializeRecommendationsAsync(CancellationToken.None);

        // Only 2 eligible users remain → each gets 1 recommendation
        var recs = await db.MatchRecommendations.ToListAsync();
        recs.Should().HaveCount(2);
        recs.Should().NotContain(r => r.UserId == matchedUserId || r.RecommendedUserId == matchedUserId);
    }

    [Fact]
    public async Task EnsureRun_RecommendationsAreRankedStartingAtOne()
    {
        await using var db = GetInMemoryDbContext();
        await SeedEligibleUsersAsync(db, count: 4);

        var builder = CreateBuilder(db);
        await builder.EnsureRunAndMaterializeRecommendationsAsync(CancellationToken.None);

        var recs = await db.MatchRecommendations.ToListAsync();

        // Group by user and verify ranks start at 1 and are sequential
        foreach (var group in recs.GroupBy(r => r.UserId))
        {
            var ranks = group.Select(r => r.Rank).OrderBy(r => r).ToList();
            ranks[0].Should().Be(1);
            for (int i = 1; i < ranks.Count; i++)
                ranks[i].Should().Be(ranks[i - 1] + 1);
        }
    }

    [Fact]
    public async Task EnsureRun_TopKLimitsRecommendationsPerUser()
    {
        await using var db = GetInMemoryDbContext();
        // Seed 10 eligible users; with TopK=3, each should get at most 3 recommendations
        await SeedEligibleUsersAsync(db, count: 10);

        var builder = CreateBuilder(db, new MatchingRunOptions { TopK = 3, ActiveWithinDays = 14, RunTtlHours = 24 });
        await builder.EnsureRunAndMaterializeRecommendationsAsync(CancellationToken.None);

        var recs = await db.MatchRecommendations.ToListAsync();
        foreach (var group in recs.GroupBy(r => r.UserId))
            group.Should().HaveCount(3);
    }

    // ──────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────

    private async Task<List<UserProfile>> SeedEligibleUsersAsync(StableFitDbContext db, int count)
    {
        var profiles = new List<UserProfile>();

        for (var i = 1; i <= count; i++)
        {
            var userId = $"user-{i}";
            var profile = UserProfile.Create(userId, $"user{i}", $"User {i}", $"user{i}@example.com");
            profile.Update(null, null, new List<DayOfWeek> { DayOfWeek.Monday }, null, null); // makes profile "complete"
            db.UserProfiles.Add(profile);

            // Each user has logged in recently
            var appUser = new AppUser
            {
                Id = userId,
                UserName = $"user{i}",
                Email = $"user{i}@example.com",
                LastLoginAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };
            db.Users.Add(appUser);
            profiles.Add(profile);
        }

        await db.SaveChangesAsync();
        return profiles;
    }
}
