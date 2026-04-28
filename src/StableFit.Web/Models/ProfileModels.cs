using System;
using System.Collections.Generic;

namespace StableFit.Web.Models;

public enum FitnessGoal
{
    WeightLoss = 0,
    MuscleGain = 1,
    MaintainWeight = 2
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public FitnessGoal Goal { get; set; }
    public List<DayOfWeek> ScheduleDays { get; set; } = new();
    public int? AgeYears { get; set; }
    public double? WeightKg { get; set; }
}

public class CreateProfileRequest
{
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    public string? Bio { get; set; }
    public FitnessGoal? Goal { get; set; }
    public List<DayOfWeek>? ScheduleDays { get; set; } = new();
    public int? AgeYears { get; set; }
    public double? WeightKg { get; set; }
}
