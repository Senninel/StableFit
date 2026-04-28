using StableFit.Domain.Enums;

namespace StableFit.Application.UserProfiles.DTOs;

public sealed record UpdateMyUserProfileRequest(
    string? Bio,
    FitnessGoal? Goal,
    List<DayOfWeek>? ScheduleDays,
    int? AgeYears,
    double? WeightKg);
