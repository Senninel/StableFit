namespace StableFit.Application.Matching.Interfaces;

public interface IMatchingRunBuilder
{
    /// <summary>
    /// Ensures there is a non-expired matching run (24h TTL) and materializes top-K recommendations per eligible user.
    /// Returns the run ID that was used/created.
    /// </summary>
    Task<Guid> EnsureRunAndMaterializeRecommendationsAsync(CancellationToken cancellationToken);
}
