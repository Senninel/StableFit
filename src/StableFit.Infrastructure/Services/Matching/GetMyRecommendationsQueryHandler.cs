using MediatR;
using Microsoft.EntityFrameworkCore;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;
using StableFit.Application.Matching.DTOs;
using StableFit.Application.Matching.Queries.GetMyRecommendations;
using StableFit.Infrastructure.Persistence;

namespace StableFit.Infrastructure.Services.Matching;

/// <summary>
/// Handles <see cref="GetMyRecommendationsQuery"/> directly in Infrastructure
/// so it can join across Infrastructure persistence entities and Domain entities
/// without violating the Application → Infrastructure dependency rule.
/// </summary>
public sealed class GetMyRecommendationsQueryHandler
    : IRequestHandler<GetMyRecommendationsQuery, IReadOnlyList<MatchRecommendationDto>>
{
    private readonly StableFitDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyRecommendationsQueryHandler(StableFitDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<MatchRecommendationDto>> Handle(
        GetMyRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var now = DateTime.UtcNow;

        // Single server-side join: recommendations → run (TTL check) → partner profile.
        // AsNoTracking throughout — this is a pure read path.
        var results = await (
            from rec in _db.MatchRecommendations.AsNoTracking()
            join run in _db.MatchRuns.AsNoTracking()
                on rec.RunId equals run.Id
            join profile in _db.UserProfiles.AsNoTracking()
                on rec.RecommendedUserId equals profile.UserId
            where rec.UserId == userId
                  && run.ExpiresAtUtc > now
            orderby rec.Rank
            select new MatchRecommendationDto(
                rec.Id,
                rec.Rank,
                rec.Score,
                profile.UserId,
                profile.Username,
                profile.Name,
                profile.Bio,
                profile.Goal,
                profile.ScheduleDays,
                profile.AgeYears,
                profile.WeightKg))
            .ToListAsync(cancellationToken);

        return results;
    }
}
