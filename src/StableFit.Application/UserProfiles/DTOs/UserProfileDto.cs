using StableFit.Domain.Enums;

namespace StableFit.Application.UserProfiles.DTOs;

public sealed record UserProfileDto(
    Guid Id,
    string UserId,
    string Username,
    string Name,
    string Email,
    string? Bio,
    FitnessGoal Goal,
    List<DayOfWeek> ScheduleDays,
    int? AgeYears,
    double? WeightKg);
