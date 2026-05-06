namespace StableFit.Infrastructure.Persistence.Entities;

public sealed class MatchRecommendation
{
    public Guid Id { get; private set; }
    public Guid RunId { get; private set; }

    /// <summary>The user receiving the recommendation list.</summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>The recommended partner for the given user.</summary>
    public string RecommendedUserId { get; private set; } = string.Empty;

    /// <summary>Rank within the user's list. 1 = best.</summary>
    public int Rank { get; private set; }

    /// <summary>Pre-computed compatibility score for transparency/debugging.</summary>
    public int Score { get; private set; }

    private MatchRecommendation(Guid id, Guid runId, string userId, string recommendedUserId, int rank, int score)
    {
        Id = id;
        RunId = runId;
        UserId = userId;
        RecommendedUserId = recommendedUserId;
        Rank = rank;
        Score = score;
    }

    private MatchRecommendation() { }

    public static MatchRecommendation Create(Guid runId, string userId, string recommendedUserId, int rank, int score)
    {
        if (runId == Guid.Empty)
            throw new ArgumentException("RunId is required.", nameof(runId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(recommendedUserId))
            throw new ArgumentException("RecommendedUserId is required.", nameof(recommendedUserId));
        if (rank <= 0)
            throw new ArgumentOutOfRangeException(nameof(rank), "Rank must be > 0.");
        if (score < 0)
            throw new ArgumentOutOfRangeException(nameof(score), "Score must be >= 0.");

        return new MatchRecommendation(Guid.NewGuid(), runId, userId.Trim(), recommendedUserId.Trim(), rank, score);
    }
}

