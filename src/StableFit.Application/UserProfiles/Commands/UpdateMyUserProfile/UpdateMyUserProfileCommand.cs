using MediatR;
using StableFit.Domain.Enums;

namespace StableFit.Application.UserProfiles.Commands.UpdateMyUserProfile;

public sealed record UpdateMyUserProfileCommand(
    string? Bio,
    FitnessGoal? Goal,
    List<DayOfWeek>? ScheduleDays,
    int? AgeYears,
    double? WeightKg,
    int? HeightCm,
    BiologicalSex? BiologicalSex,
    int? DailyCalorieTarget) : IRequest;
