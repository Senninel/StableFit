using System;
using System.Collections.Generic;

namespace StableFit.Web.Models;

public enum MatchDecisionType
{
    None = 0,
    Accepted = 1,
    Rejected = 2
}

public class MatchRecommendationDto
{
    public Guid RecommendationId { get; set; }
    public int Rank { get; set; }
    public int Score { get; set; }
    
    public string PartnerUserId { get; set; } = string.Empty;
    public string PartnerUsername { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public string? PartnerBio { get; set; }
    public FitnessGoal PartnerGoal { get; set; }
    public List<DayOfWeek> PartnerScheduleDays { get; set; } = new();
    public int? PartnerAgeYears { get; set; }
    public double? PartnerWeightKg { get; set; }
}

public class ActiveMatchDto
{
    public Guid MatchId { get; set; }
    public DateTime MatchedAtUtc { get; set; }
    
    public string PartnerUserId { get; set; } = string.Empty;
    public string PartnerUsername { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public string? PartnerBio { get; set; }
    public FitnessGoal PartnerGoal { get; set; }
    public List<DayOfWeek> PartnerScheduleDays { get; set; } = new();
    public int? PartnerAgeYears { get; set; }
    public double? PartnerWeightKg { get; set; }
}

public class SubmitDecisionRequest
{
    public Guid RecommendationId { get; set; }
    public MatchDecisionType Decision { get; set; }
}

public class SubmitDecisionResponse
{
    public bool MatchFormed { get; set; }
    public Guid? MatchId { get; set; }
}
