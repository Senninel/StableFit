using StableFit.Domain.Enums;

namespace StableFit.Infrastructure.Persistence.Entities;

public sealed class Match
{
    public Guid Id { get; private set; }

    public string UserId1 { get; private set; } = string.Empty;
    public string UserId2 { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public MatchStatus Status { get; private set; }

    private Match(Guid id, string userId1, string userId2, DateTime createdAtUtc, MatchStatus status)
    {
        Id = id;
        UserId1 = userId1;
        UserId2 = userId2;
        CreatedAtUtc = createdAtUtc;
        Status = status;
    }

    private Match() { }

    public static Match CreateActive(string userId1, string userId2, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId1))
            throw new ArgumentException("UserId1 is required.", nameof(userId1));
        if (string.IsNullOrWhiteSpace(userId2))
            throw new ArgumentException("UserId2 is required.", nameof(userId2));

        var a = userId1.Trim();
        var b = userId2.Trim();

        if (string.Equals(a, b, StringComparison.Ordinal))
            throw new ArgumentException("UserId1 and UserId2 cannot be the same.");

        var created = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

        // Deterministic ordering to simplify uniqueness checks.
        if (string.CompareOrdinal(a, b) > 0)
            (a, b) = (b, a);

        return new Match(Guid.NewGuid(), a, b, created, MatchStatus.Active);
    }

    public void End(DateTime utcNow)
    {
        Status = MatchStatus.Ended;

        // We keep CreatedAtUtc immutable in meaning (creation time), so don't overwrite it.
        // If you later want an audit timestamp, add EndedAtUtc instead.
    }
}
