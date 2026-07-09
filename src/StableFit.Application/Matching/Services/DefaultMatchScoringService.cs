using StableFit.Application.Matching.Interfaces;
using StableFit.Domain.Entities;

namespace StableFit.Application.Matching.Services;

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
        score += ScoreGymProximity(a, b);

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

    // Max 30 pts — rewards users who train at or near the same gym.
    private static int ScoreGymProximity(UserProfile a, UserProfile b)
    {
        if (a.PreferredGymLatitude is null || b.PreferredGymLatitude is null) return 0;

        var km = HaversineKm(
            a.PreferredGymLatitude.Value, a.PreferredGymLongitude!.Value,
            b.PreferredGymLatitude.Value, b.PreferredGymLongitude!.Value);

        return km switch
        {
            <= 0.1 => 30,
            <= 1.0 => 20,
            <= 5.0 => 10,
            _ => 0
        };
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
