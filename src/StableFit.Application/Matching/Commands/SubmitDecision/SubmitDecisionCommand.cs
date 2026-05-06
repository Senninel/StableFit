using MediatR;
using StableFit.Domain.Enums;

namespace StableFit.Application.Matching.Commands.SubmitDecision;

/// <summary>
/// Records a Like or Dislike decision for a recommendation.
/// When both users Like each other, a <see cref="StableFit.Infrastructure.Persistence.Entities.Match"/> is automatically created.
/// </summary>
public sealed record SubmitDecisionCommand(
    Guid RecommendationId,
    MatchDecisionType Decision) : IRequest<SubmitDecisionResult>;

/// <summary>Result carrier — tells the caller whether a mutual match was formed.</summary>
public sealed record SubmitDecisionResult(bool MatchFormed, Guid? MatchId);

