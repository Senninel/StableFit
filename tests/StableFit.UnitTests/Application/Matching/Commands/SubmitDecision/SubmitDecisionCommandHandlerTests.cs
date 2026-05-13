using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;
using StableFit.Application.Matching.Commands.SubmitDecision;
using StableFit.Domain.Enums;
using StableFit.Infrastructure.Persistence;
using StableFit.Infrastructure.Persistence.Entities;
using StableFit.Infrastructure.Services.Matching;

namespace StableFit.UnitTests.Application.Matching.Commands.SubmitDecision;

public class SubmitDecisionCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserMock;
    private readonly ILogger<SubmitDecisionCommandHandler> _loggerMock;
    private readonly string _fromUserId = "user-1";
    private readonly string _toUserId = "user-2";
    private readonly Guid _runId = Guid.NewGuid();

    public SubmitDecisionCommandHandlerTests()
    {
        _currentUserMock = Substitute.For<ICurrentUserService>();
        _loggerMock = Substitute.For<ILogger<SubmitDecisionCommandHandler>>();
        
        _currentUserMock.UserId.Returns(_fromUserId);
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
        var handler = new SubmitDecisionCommandHandler(db, _currentUserMock, _loggerMock);

        var command = new SubmitDecisionCommand(Guid.NewGuid(), MatchDecisionType.Accepted);

        await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_RecommendationNotFoundOrExpired_ThrowsNotFoundException()
    {
        await using var db = GetInMemoryDbContext();
        var handler = new SubmitDecisionCommandHandler(db, _currentUserMock, _loggerMock);

        var command = new SubmitDecisionCommand(Guid.NewGuid(), MatchDecisionType.Accepted);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DuplicateSameDecision_ReturnsFalseIdempotent()
    {
        await using var db = GetInMemoryDbContext();
        var recId = await SeedValidRecommendation(db);
        
        // Seed duplicate decision
        db.MatchDecisions.Add(MatchDecision.Create(_runId, _fromUserId, _toUserId, MatchDecisionType.Accepted, DateTime.UtcNow));
        await db.SaveChangesAsync();

        var handler = new SubmitDecisionCommandHandler(db, _currentUserMock, _loggerMock);
        var command = new SubmitDecisionCommand(recId, MatchDecisionType.Accepted);

        var result = await handler.Handle(command, CancellationToken.None);

        result.MatchFormed.Should().BeFalse();
        result.MatchId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DuplicateDifferentDecision_ThrowsConflictException()
    {
        await using var db = GetInMemoryDbContext();
        var recId = await SeedValidRecommendation(db);
        
        // Seed conflicting decision
        db.MatchDecisions.Add(MatchDecision.Create(_runId, _fromUserId, _toUserId, MatchDecisionType.Rejected, DateTime.UtcNow));
        await db.SaveChangesAsync();

        var handler = new SubmitDecisionCommandHandler(db, _currentUserMock, _loggerMock);
        var command = new SubmitDecisionCommand(recId, MatchDecisionType.Accepted);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_RejectedDecision_SavesDecisionAndReturnsFalse()
    {
        await using var db = GetInMemoryDbContext();
        var recId = await SeedValidRecommendation(db);

        var handler = new SubmitDecisionCommandHandler(db, _currentUserMock, _loggerMock);
        var command = new SubmitDecisionCommand(recId, MatchDecisionType.Rejected);

        var result = await handler.Handle(command, CancellationToken.None);

        result.MatchFormed.Should().BeFalse();
        result.MatchId.Should().BeNull();

        var savedDecision = await db.MatchDecisions.SingleOrDefaultAsync(d => d.FromUserId == _fromUserId && d.ToUserId == _toUserId);
        savedDecision.Should().NotBeNull();
        savedDecision!.Decision.Should().Be(MatchDecisionType.Rejected);
    }

    [Fact]
    public async Task Handle_AcceptedButCounterpartNotAccepted_SavesDecisionAndReturnsFalse()
    {
        await using var db = GetInMemoryDbContext();
        var recId = await SeedValidRecommendation(db);

        var handler = new SubmitDecisionCommandHandler(db, _currentUserMock, _loggerMock);
        var command = new SubmitDecisionCommand(recId, MatchDecisionType.Accepted);

        var result = await handler.Handle(command, CancellationToken.None);

        result.MatchFormed.Should().BeFalse();
        result.MatchId.Should().BeNull();

        var savedDecision = await db.MatchDecisions.SingleOrDefaultAsync(d => d.FromUserId == _fromUserId && d.ToUserId == _toUserId);
        savedDecision.Should().NotBeNull();
        savedDecision!.Decision.Should().Be(MatchDecisionType.Accepted);
    }

    [Fact]
    public async Task Handle_MutualAcceptButUserHasActiveMatch_ReturnsFalse()
    {
        await using var db = GetInMemoryDbContext();
        var recId = await SeedValidRecommendation(db);
        
        // Counterpart accepted
        db.MatchDecisions.Add(MatchDecision.Create(_runId, _toUserId, _fromUserId, MatchDecisionType.Accepted, DateTime.UtcNow));
        
        // Caller already has a match
        db.Matches.Add(Match.CreateActive(_fromUserId, "user-3", DateTime.UtcNow));
        await db.SaveChangesAsync();

        var handler = new SubmitDecisionCommandHandler(db, _currentUserMock, _loggerMock);
        var command = new SubmitDecisionCommand(recId, MatchDecisionType.Accepted);

        var result = await handler.Handle(command, CancellationToken.None);

        result.MatchFormed.Should().BeFalse();
        result.MatchId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_MutualAccept_CreatesMatchAndReturnsTrue()
    {
        await using var db = GetInMemoryDbContext();
        var recId = await SeedValidRecommendation(db);
        
        // Counterpart accepted
        db.MatchDecisions.Add(MatchDecision.Create(_runId, _toUserId, _fromUserId, MatchDecisionType.Accepted, DateTime.UtcNow));
        await db.SaveChangesAsync();

        var handler = new SubmitDecisionCommandHandler(db, _currentUserMock, _loggerMock);
        var command = new SubmitDecisionCommand(recId, MatchDecisionType.Accepted);

        var result = await handler.Handle(command, CancellationToken.None);

        result.MatchFormed.Should().BeTrue();
        result.MatchId.Should().NotBeNull();

        var match = await db.Matches.SingleOrDefaultAsync(m => m.Id == result.MatchId);
        match.Should().NotBeNull();
        match!.Status.Should().Be(MatchStatus.Active);
        
        // The match should be between fromUser and toUser
        var usersInMatch = new[] { match.UserId1, match.UserId2 };
        usersInMatch.Should().Contain(_fromUserId);
        usersInMatch.Should().Contain(_toUserId);
    }

    private async Task<Guid> SeedValidRecommendation(StableFitDbContext db)
    {
        var run = MatchRun.Create(DateTime.UtcNow, TimeSpan.FromDays(2), 10);
        run.GetType().GetProperty(nameof(MatchRun.Id))!.SetValue(run, _runId);
        
        var recommendation = MatchRecommendation.Create(_runId, _fromUserId, _toUserId, 1, 95);
        
        db.MatchRuns.Add(run);
        db.MatchRecommendations.Add(recommendation);
        await db.SaveChangesAsync();

        return recommendation.Id;
    }
}
