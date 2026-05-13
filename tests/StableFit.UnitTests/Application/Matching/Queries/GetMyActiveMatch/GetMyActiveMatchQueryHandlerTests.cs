using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;
using StableFit.Application.Matching.Queries.GetMyActiveMatch;
using StableFit.Domain.Entities;
using StableFit.Infrastructure.Persistence;
using StableFit.Infrastructure.Persistence.Entities;
using StableFit.Infrastructure.Services.Matching;

namespace StableFit.UnitTests.Application.Matching.Queries.GetMyActiveMatch;

public class GetMyActiveMatchQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserMock;
    private readonly string _userId = "user-1";

    public GetMyActiveMatchQueryHandlerTests()
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
        var handler = new GetMyActiveMatchQueryHandler(db, _currentUserMock);
        var query = new GetMyActiveMatchQuery();

        await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NoActiveMatch_ReturnsNull()
    {
        await using var db = GetInMemoryDbContext();
        var handler = new GetMyActiveMatchQueryHandler(db, _currentUserMock);
        var query = new GetMyActiveMatchQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_HasActiveMatchAsUser1_ReturnsPartnerProfile()
    {
        await using var db = GetInMemoryDbContext();
        
        var partnerId = "user-2";
        var partnerProfile = UserProfile.Create(partnerId, "partner2", "Partner Two", "partner@example.com");
        db.UserProfiles.Add(partnerProfile);

        // Caller is UserId1
        var match = Match.CreateActive(_userId, partnerId, DateTime.UtcNow);
        db.Matches.Add(match);

        await db.SaveChangesAsync();

        var handler = new GetMyActiveMatchQueryHandler(db, _currentUserMock);
        var query = new GetMyActiveMatchQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.MatchId.Should().Be(match.Id);
        result.PartnerUserId.Should().Be(partnerId);
        result.PartnerUsername.Should().Be("partner2");
    }

    [Fact]
    public async Task Handle_HasActiveMatchAsUser2_ReturnsPartnerProfile()
    {
        await using var db = GetInMemoryDbContext();
        
        var partnerId = "user-3";
        var partnerProfile = UserProfile.Create(partnerId, "partner3", "Partner Three", "partner@example.com");
        db.UserProfiles.Add(partnerProfile);

        // Caller is UserId2
        var match = Match.CreateActive(partnerId, _userId, DateTime.UtcNow);
        db.Matches.Add(match);

        await db.SaveChangesAsync();

        var handler = new GetMyActiveMatchQueryHandler(db, _currentUserMock);
        var query = new GetMyActiveMatchQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.MatchId.Should().Be(match.Id);
        result.PartnerUserId.Should().Be(partnerId);
        result.PartnerUsername.Should().Be("partner3");
    }

    [Fact]
    public async Task Handle_ActiveMatchMissingPartnerProfile_ReturnsNull()
    {
        await using var db = GetInMemoryDbContext();
        
        var partnerId = "user-missing";
        
        // Match exists, but UserProfile for partner does NOT exist
        var match = Match.CreateActive(_userId, partnerId, DateTime.UtcNow);
        db.Matches.Add(match);

        await db.SaveChangesAsync();

        var handler = new GetMyActiveMatchQueryHandler(db, _currentUserMock);
        var query = new GetMyActiveMatchQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
