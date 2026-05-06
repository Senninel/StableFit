using StableFit.Domain.Enums;

namespace StableFit.Infrastructure.Persistence.Entities;

public sealed class MatchDecision
{
    public Guid Id { get; private set; }
    public Guid RunId { get; private set; }

    public string FromUserId { get; private set; } = string.Empty;
    public string ToUserId { get; private set; } = string.Empty;

    public MatchDecisionType Decision { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private MatchDecision(Guid id, Guid runId, string fromUserId, string toUserId, MatchDecisionType decision, DateTime createdAtUtc)
    {
        Id = id;
        RunId = runId;
        FromUserId = fromUserId;
        ToUserId = toUserId;
        Decision = decision;
        CreatedAtUtc = createdAtUtc;
    }

    private MatchDecision() { }

    public static MatchDecision Create(Guid runId, string fromUserId, string toUserId, MatchDecisionType decision, DateTime utcNow)
    {
        if (runId == Guid.Empty)
            throw new ArgumentException("RunId is required.", nameof(runId));
        if (string.IsNullOrWhiteSpace(fromUserId))
            throw new ArgumentException("FromUserId is required.", nameof(fromUserId));
        if (string.IsNullOrWhiteSpace(toUserId))
            throw new ArgumentException("ToUserId is required.", nameof(toUserId));
        if (string.Equals(fromUserId.Trim(), toUserId.Trim(), StringComparison.Ordinal))
            throw new ArgumentException("FromUserId and ToUserId cannot be the same.");

        var created = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
        return new MatchDecision(Guid.NewGuid(), runId, fromUserId.Trim(), toUserId.Trim(), decision, created);
    }

    public void UpdateDecision(MatchDecisionType decision, DateTime utcNow)
    {
        Decision = decision;
        CreatedAtUtc = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
    }
}

