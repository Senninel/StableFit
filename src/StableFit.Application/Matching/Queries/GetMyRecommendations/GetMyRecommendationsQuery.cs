using MediatR;
using StableFit.Application.Matching.DTOs;

namespace StableFit.Application.Matching.Queries.GetMyRecommendations;

/// <summary>
/// Returns the current user's ranked recommendation list from the latest active matching run.
/// Returns an empty list when no run exists or it has expired.
/// </summary>
public sealed record GetMyRecommendationsQuery : IRequest<IReadOnlyList<MatchRecommendationDto>>;

