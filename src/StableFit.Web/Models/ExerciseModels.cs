namespace StableFit.Web.Models;

public record ExerciseAnalysisResult(
    int FrameCount,
    double Fps,
    string ExerciseType,
    int Score,
    int RepCount,
    IReadOnlyList<string> Feedback);
