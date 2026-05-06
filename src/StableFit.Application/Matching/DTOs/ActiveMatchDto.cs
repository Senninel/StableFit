using System;
using System.Collections.Generic;
using StableFit.Domain.Enums;

namespace StableFit.Application.Matching.DTOs;

public record ActiveMatchDto(
    Guid MatchId,
    DateTime MatchedAtUtc,
    string PartnerUserId,
    string PartnerUsername,
    string PartnerName,
    string? PartnerBio,
    FitnessGoal PartnerGoal,
    IReadOnlyCollection<DayOfWeek> PartnerScheduleDays,
    int? PartnerAgeYears,
    double? PartnerWeightKg
);
