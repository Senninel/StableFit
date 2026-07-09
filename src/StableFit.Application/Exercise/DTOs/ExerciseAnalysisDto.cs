namespace StableFit.Application.Exercise.DTOs;

public record ExerciseAnalysisDto
{
    public int FrameCount { get; init; }
    public double Fps { get; init; }
    public string ExerciseType { get; init; } = string.Empty;
    public int Score { get; init; }
    public int RepCount { get; init; }
    public IReadOnlyList<string> Feedback { get; init; } = [];
}
