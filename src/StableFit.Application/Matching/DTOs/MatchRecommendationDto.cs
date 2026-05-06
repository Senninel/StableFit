using StableFit.Domain.Enums;

namespace StableFit.Application.Matching.DTOs;

/// <summary>
/// A single ranked recommendation for the current user.
/// Includes the partner's public profile data plus the pre-computed score.
/// </summary>
public sealed record MatchRecommendationDto(
    Guid RecommendationId,
    int Rank,
    int Score,
    string PartnerUserId,
    string PartnerUsername,
    string PartnerName,
    string? PartnerBio,
    FitnessGoal? PartnerGoal,
    IReadOnlyList<DayOfWeek> PartnerScheduleDays,
    int? PartnerAgeYears,
    double? PartnerWeightKg);
