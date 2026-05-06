using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StableFit.Application.Matching.Interfaces;
using StableFit.Domain.Entities;
using StableFit.Domain.Enums;
using StableFit.Infrastructure.Persistence;
using StableFit.Infrastructure.Persistence.Entities;

namespace StableFit.Infrastructure.Services.Matching;

public sealed class MatchingRunBuilder : IMatchingRunBuilder
{
    private readonly StableFitDbContext _db;
    private readonly IMatchScoringService _scoring;
    private readonly MatchingRunOptions _options;

    public MatchingRunBuilder(
        StableFitDbContext db,
        IMatchScoringService scoring,
        IOptions<MatchingRunOptions> options)
    {
        _db = db;
        _scoring = scoring;
        _options = options.Value;
    }

    public async Task<Guid> EnsureRunAndMaterializeRecommendationsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var ttl = TimeSpan.FromHours(_options.RunTtlHours);

        // 1) Reuse latest non-expired run
        var existingRun = await _db.MatchRuns
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAtUtc)
            .FirstOrDefaultAsync(r => r.ExpiresAtUtc > now, cancellationToken);

        if (existingRun is not null)
            return existingRun.Id;

        // 2) Build eligible pool
        var profiles = await LoadEligibleProfilesAsync(now, cancellationToken);

        // 3) Create run and materialize recommendations
        var run = MatchRun.Create(now, ttl, profiles.Count);
        _db.MatchRuns.Add(run);

        var recommendations = BuildRecommendations(run.Id, profiles, _options.TopK);
        _db.MatchRecommendations.AddRange(recommendations);

        await _db.SaveChangesAsync(cancellationToken);
        return run.Id;
    }

    private async Task<List<UserProfile>> LoadEligibleProfilesAsync(DateTime nowUtc, CancellationToken ct)
    {
        var cutoff = nowUtc.AddDays(-_options.ActiveWithinDays);

        // Active match user IDs — use Union instead of SelectMany(array) which EF can't translate
        var activeMatchUserIds = await _db.Matches
            .AsNoTracking()
            .Where(m => m.Status == MatchStatus.Active)
            .Select(m => m.UserId1)
            .Union(_db.Matches
                .AsNoTracking()
                .Where(m => m.Status == MatchStatus.Active)
                .Select(m => m.UserId2))
            .ToListAsync(ct);

        var activeMatchSet = activeMatchUserIds.ToHashSet(StringComparer.Ordinal);

        // Join profiles with identity users to filter by LastLoginAt
        var profiles = await (from p in _db.UserProfiles.AsNoTracking()
                              join u in _db.Users.AsNoTracking() on p.UserId equals u.Id
                              where u.LastLoginAt != null && u.LastLoginAt >= cutoff
                              select p)
            .ToListAsync(ct);

        // Complete profile = at least 1 schedule day
        return profiles
            .Where(p => p.ScheduleDays.Count > 0)
            .Where(p => !activeMatchSet.Contains(p.UserId))
            .ToList();
    }

    private IReadOnlyList<MatchRecommendation> BuildRecommendations(Guid runId, IReadOnlyList<UserProfile> profiles, int topK)
    {
        if (topK <= 0) return Array.Empty<MatchRecommendation>();
        if (profiles.Count < 2) return Array.Empty<MatchRecommendation>();

        var results = new List<MatchRecommendation>(profiles.Count * topK);

        // Deterministic ordering
        var ordered = profiles.OrderBy(p => p.Id).ToList();

        foreach (var me in ordered)
        {
            var ranked = ordered
                .Where(p => p.UserId != me.UserId)
                .Select(p => new { Profile = p, Score = _scoring.Score(me, p) })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Profile.Id)
                .Take(topK)
                .ToList();

            var rank = 1;
            foreach (var candidate in ranked)
            {
                results.Add(MatchRecommendation.Create(
                    runId,
                    me.UserId,
                    candidate.Profile.UserId,
                    rank,
                    candidate.Score));

                rank++;
            }
        }

        return results;
    }
}

