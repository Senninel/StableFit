using StableFit.Domain.Entities;

namespace StableFit.Application.Matching.Interfaces;

public interface IMatchScoringService
{
    /// <summary>
    /// Returns a non-negative compatibility score between two user profiles. Higher means more compatible.
    /// </summary>
    int Score(UserProfile a, UserProfile b);
}

