namespace StableFit.Infrastructure.Services.Matching;

public sealed class MatchingRunOptions
{
    public int TopK { get; init; } = 5;
    public int ActiveWithinDays { get; init; } = 14;
    public int RunTtlHours { get; init; } = 24;
}
