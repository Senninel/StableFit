using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;
using StableFit.Application.Matching.Queries.GetMyRecommendations;
using StableFit.Domain.Entities;
using StableFit.Infrastructure.Persistence;
using StableFit.Infrastructure.Persistence.Entities;
using StableFit.Infrastructure.Services.Matching;

namespace StableFit.UnitTests.Application.Matching.Queries.GetMyRecommendations;

public class GetMyRecommendationsQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserMock;
    private readonly string _userId = "user-1";

    public GetMyRecommendationsQueryHandlerTests()
    {
        _currentUserMock = Substitute.For<ICurrentUserService>();
        _currentUserMock.UserId.Returns(_userId);
    }

    private StableFitDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<StableFitDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new StableFitDbContext(options);
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ThrowsUnauthorizedException()
    {
        _currentUserMock.UserId.Returns((string?)null);
        await using var db = GetInMemoryDbContext();
        var handler = new GetMyRecommendationsQueryHandler(db, _currentUserMock);
        var query = new GetMyRecommendationsQuery();

        await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReturnsValidRecommendationsOrderedByRank()
    {
        await using var db = GetInMemoryDbContext();
        
        var validRun = MatchRun.Create(DateTime.UtcNow, TimeSpan.FromDays(2), 10);
        db.MatchRuns.Add(validRun);

        var expiredRun = MatchRun.Create(DateTime.UtcNow.AddDays(-5), TimeSpan.FromDays(1), 10);
        db.MatchRuns.Add(expiredRun);

        // Partner 1 profile
        var partner1 = UserProfile.Create("partner-1", "partner1", "Partner One", "partner1@example.com");
        db.UserProfiles.Add(partner1);
        
        // Partner 2 profile
        var partner2 = UserProfile.Create("partner-2", "partner2", "Partner Two", "partner2@example.com");
        db.UserProfiles.Add(partner2);

        // Recommendation 1: Valid run, Rank 2
        var rec1 = MatchRecommendation.Create(validRun.Id, _userId, "partner-1", 2, 85);
        db.MatchRecommendations.Add(rec1);

        // Recommendation 2: Valid run, Rank 1
        var rec2 = MatchRecommendation.Create(validRun.Id, _userId, "partner-2", 1, 95);
        db.MatchRecommendations.Add(rec2);

        // Recommendation 3: Expired run
        var rec3 = MatchRecommendation.Create(expiredRun.Id, _userId, "partner-1", 1, 90);
        db.MatchRecommendations.Add(rec3);

        // Recommendation 4: Different user
        var rec4 = MatchRecommendation.Create(validRun.Id, "other-user", "partner-1", 1, 90);
        db.MatchRecommendations.Add(rec4);

        await db.SaveChangesAsync();

        var handler = new GetMyRecommendationsQueryHandler(db, _currentUserMock);
        var query = new GetMyRecommendationsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Only rec1 and rec2

        // Should be ordered by rank ascending (Rank 1 first)
        result[0].Rank.Should().Be(1);
        result[0].PartnerUserId.Should().Be("partner-2");
        result[0].PartnerUsername.Should().Be("partner2");

        result[1].Rank.Should().Be(2);
        result[1].PartnerUserId.Should().Be("partner-1");
        result[1].PartnerUsername.Should().Be("partner1");
    }

    [Fact]
    public async Task Handle_NoValidRecommendations_ReturnsEmptyList()
    {
        await using var db = GetInMemoryDbContext();
        var handler = new GetMyRecommendationsQueryHandler(db, _currentUserMock);
        var query = new GetMyRecommendationsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
