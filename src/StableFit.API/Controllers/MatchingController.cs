using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StableFit.Application.Matching.Commands.EnsureMatchingRun;
using StableFit.Application.Matching.Commands.SubmitDecision;
using StableFit.Application.Matching.Queries.GetMyRecommendations;

namespace StableFit.API.Controllers;

[ApiController]
[Route("api/matching")]
[Authorize]
public sealed class MatchingController : ControllerBase
{
    private readonly ISender _sender;

    public MatchingController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Ensures the current 24h matching run exists and materializes top-K recommendations for all eligible users.
    /// Returns the run Id.
    ///
    /// Note: For MVP this endpoint is protected but not role-restricted.
    /// In later phases you may want to require an admin role or run it via a background job.
    /// </summary>
    [HttpPost("run/ensure")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> EnsureRun(CancellationToken ct)
    {
        var runId = await _sender.Send(new EnsureMatchingRunCommand(), ct);
        return Accepted(new { runId });
    }

    /// <summary>
    /// Returns the current user's ranked recommendation list from the latest active matching run.
    /// Returns an empty array when no active run exists.
    /// </summary>
    [HttpGet("recommendations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyRecommendations(CancellationToken ct)
    {
        var recommendations = await _sender.Send(new GetMyRecommendationsQuery(), ct);
        return Ok(recommendations);
    }

    /// <summary>
    /// Returns the current user's active match if one exists.
    /// Returns 404 Not Found if the user is unmatched.
    /// </summary>
    [HttpGet("my-match")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyActiveMatch(CancellationToken ct)
    {
        var match = await _sender.Send(new StableFit.Application.Matching.Queries.GetMyActiveMatch.GetMyActiveMatchQuery(), ct);
        if (match is null)
            return NotFound();

        return Ok(match);
    }

    /// <summary>
    /// Submits a Like or Dislike decision for a recommendation.
    /// Returns 201 with the match ID when a mutual like forms a match; 200 otherwise.
    /// </summary>
    [HttpPost("decisions")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitDecision(
        [FromBody] SubmitDecisionCommand command,
        CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);

        if (result.MatchFormed)
            return Created($"/api/matching/matches/{result.MatchId}", new { result.MatchId });

        return Ok(new { result.MatchFormed });
    }
}


