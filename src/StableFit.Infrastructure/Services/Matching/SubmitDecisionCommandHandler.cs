using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;
using StableFit.Application.Matching.Commands.SubmitDecision;
using StableFit.Domain.Enums;
using StableFit.Infrastructure.Persistence;
using StableFit.Infrastructure.Persistence.Entities;

namespace StableFit.Infrastructure.Services.Matching;

/// <summary>
/// Handles <see cref="SubmitDecisionCommand"/>.
///
/// Business rules:
///   1. The recommendation must belong to the current run (non-expired) and to the caller.
///   2. A duplicate decision for the same run/from/to triplet is rejected (409).
///   3. If the decision is Accepted and the counterpart has also Accepted, a Match is created.
///   4. A user pair can only have one Active match at a time (unique constraint enforced in DB).
/// </summary>
public sealed class SubmitDecisionCommandHandler : IRequestHandler<SubmitDecisionCommand, SubmitDecisionResult>
{
    private readonly StableFitDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<SubmitDecisionCommandHandler> _logger;

    public SubmitDecisionCommandHandler(
        StableFitDbContext db,
        ICurrentUserService currentUser,
        ILogger<SubmitDecisionCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<SubmitDecisionResult> Handle(
        SubmitDecisionCommand request,
        CancellationToken cancellationToken)
    {
        var fromUserId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var now = DateTime.UtcNow;

        // 1) Load the recommendation — must belong to this user and be in a live run.
        var recommendation = await _db.MatchRecommendations
            .AsNoTracking()
            .Join(_db.MatchRuns.AsNoTracking(),
                rec => rec.RunId,
                run => run.Id,
                (rec, run) => new { Recommendation = rec, Run = run })
            .Where(x => x.Recommendation.Id == request.RecommendationId
                        && x.Recommendation.UserId == fromUserId
                        && x.Run.ExpiresAtUtc > now)
            .Select(x => new
            {
                x.Recommendation.RunId,
                x.Recommendation.RecommendedUserId
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(MatchRecommendation), request.RecommendationId);

        var toUserId = recommendation.RecommendedUserId;
        var runId = recommendation.RunId;

        // 2) Idempotency / duplicate guard — one decision per (run, from, to) triplet.
        var existingDecision = await _db.MatchDecisions
            .FirstOrDefaultAsync(
                d => d.RunId == runId && d.FromUserId == fromUserId && d.ToUserId == toUserId,
                cancellationToken);

        if (existingDecision is not null)
        {
            // Allow updating to the same value silently (idempotent); reject a change.
            if (existingDecision.Decision == request.Decision)
                return new SubmitDecisionResult(false, null);

            throw new ConflictException(
                $"A decision already exists for this recommendation. Existing: {existingDecision.Decision}.");
        }

        // 3) Persist the decision.
        var decision = MatchDecision.Create(runId, fromUserId, toUserId, request.Decision, now);
        _db.MatchDecisions.Add(decision);

        // 4) Check for mutual like — only if the caller accepted.
        if (request.Decision != MatchDecisionType.Accepted)
        {
            await _db.SaveChangesAsync(cancellationToken);
            return new SubmitDecisionResult(false, null);
        }

        var counterpartAccepted = await _db.MatchDecisions
            .AnyAsync(
                d => d.RunId == runId
                     && d.FromUserId == toUserId
                     && d.ToUserId == fromUserId
                     && d.Decision == MatchDecisionType.Accepted,
                cancellationToken);

        if (!counterpartAccepted)
        {
            await _db.SaveChangesAsync(cancellationToken);
            return new SubmitDecisionResult(false, null);
        }

        // 5) Mutual match — enforce strict "One Gym Buddy Only" rule.
        // If EITHER user already has an active match with anyone, do not form a new one.
        var eitherUserHasActiveMatch = await _db.Matches
            .AnyAsync(
                m => m.Status == MatchStatus.Active
                     && (m.UserId1 == fromUserId || m.UserId2 == fromUserId
                         || m.UserId1 == toUserId || m.UserId2 == toUserId),
                cancellationToken);

        if (eitherUserHasActiveMatch)
        {
            // Both liked, but at least one of them is already paired up.
            // Persist the decision, but do not create a new Match.
            _logger.LogWarning(
                "Mutual like detected, but one or both users ({UserId1}, {UserId2}) already have an active match.",
                fromUserId, toUserId);

            await _db.SaveChangesAsync(cancellationToken);
            return new SubmitDecisionResult(false, null);
        }

        var match = Match.CreateActive(fromUserId, toUserId, now);
        _db.Matches.Add(match);

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Match {MatchId} created for users {UserId1} and {UserId2}.",
            match.Id, fromUserId, toUserId);

        return new SubmitDecisionResult(true, match.Id);
    }
}
