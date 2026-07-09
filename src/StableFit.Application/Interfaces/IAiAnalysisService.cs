using StableFit.Application.Exercise.DTOs;

namespace StableFit.Application.Interfaces;

public interface IAiAnalysisService
{
    Task<ExerciseAnalysisDto> AnalyzeAsync(
        byte[] videoBytes,
        string fileName,
        string exerciseType,
        CancellationToken cancellationToken = default);
}
