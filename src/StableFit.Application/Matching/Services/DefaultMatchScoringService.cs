using StableFit.Application.Matching.Interfaces;
using StableFit.Domain.Entities;

namespace StableFit.Application.Matching.Services;

/// <summary>
/// MVP scoring model for Weeks 5-6.
///
/// Uses only data we already have in <see cref="UserProfile"/>:
/// - primary goal
/// - schedule overlap
/// - age closeness (optional)
/// - weight closeness (optional)
///
/// Later phases can extend this with proximity/gyms, music taste, embedding similarity, etc.
/// </summary>
public sealed class DefaultMatchScoringService : IMatchScoringService
{
    public int Score(UserProfile a, UserProfile b)
    {
        if (a.Id == b.Id) return 0;

        var score = 0;

        score += ScoreGoal(a, b);
        score += ScoreScheduleOverlap(a, b);
        score += ScoreAgeCloseness(a, b);
        score += ScoreWeightCloseness(a, b);

        return Math.Max(0, score);
    }

    private static int ScoreGoal(UserProfile a, UserProfile b)
        => a.Goal == b.Goal ? 50 : 0;

    private static int ScoreScheduleOverlap(UserProfile a, UserProfile b)
    {
        var overlap = CountOverlapDays(a.ScheduleDays, b.ScheduleDays);

        // 0..40 (cap) in steps of 10
        return Math.Min(overlap * 10, 40);
    }

    private static int ScoreAgeCloseness(UserProfile a, UserProfile b)
    {
        if (!a.AgeYears.HasValue || !b.AgeYears.HasValue) return 0;

        var diff = Math.Abs(a.AgeYears.Value - b.AgeYears.Value);

        if (diff <= 2) return 15;
        if (diff <= 5) return 10;
        if (diff <= 10) return 5;
        return 0;
    }

    private static int ScoreWeightCloseness(UserProfile a, UserProfile b)
    {
        if (!a.WeightKg.HasValue || !b.WeightKg.HasValue) return 0;

        var diff = Math.Abs(a.WeightKg.Value - b.WeightKg.Value);

        if (diff <= 5) return 10;
        if (diff <= 10) return 5;
        return 0;
    }

    private static int CountOverlapDays(IReadOnlyCollection<DayOfWeek>? a, IReadOnlyCollection<DayOfWeek>? b)
    {
        if (a is null || b is null || a.Count == 0 || b.Count == 0) return 0;

        var set = new HashSet<DayOfWeek>(a);
        var overlap = 0;
        foreach (var day in b)
        {
            if (set.Contains(day))
                overlap++;
        }

        return overlap;
    }
}
