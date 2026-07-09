using MediatR;
using StableFit.Application.Exercise.DTOs;

namespace StableFit.Application.Exercise.Commands.AnalyzeExercise;

public record AnalyzeExerciseCommand : IRequest<ExerciseAnalysisDto>
{
    public required byte[] VideoBytes { get; init; }
    public required string FileName { get; init; }
    public required string ExerciseType { get; init; }
}
