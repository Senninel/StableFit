namespace StableFit.Infrastructure.Persistence.Entities;

public sealed class MatchRun
{
    public Guid Id { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public int EligibleUserCount { get; private set; }

    private MatchRun(Guid id, DateTime createdAtUtc, DateTime expiresAtUtc, int eligibleUserCount)
    {
        Id = id;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        EligibleUserCount = eligibleUserCount;
    }

    private MatchRun() { }

    public static MatchRun Create(DateTime utcNow, TimeSpan ttl, int eligibleUserCount)
    {
        if (eligibleUserCount < 0)
            throw new ArgumentOutOfRangeException(nameof(eligibleUserCount));

        var created = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
        return new MatchRun(Guid.NewGuid(), created, created.Add(ttl), eligibleUserCount);
    }
}

