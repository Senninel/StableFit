using MediatR;
using StableFit.Application.Exercise.DTOs;
using StableFit.Application.Interfaces;

namespace StableFit.Application.Exercise.Commands.AnalyzeExercise;

public sealed class AnalyzeExerciseCommandHandler : IRequestHandler<AnalyzeExerciseCommand, ExerciseAnalysisDto>
{
    private readonly IAiAnalysisService _aiAnalysisService;

    public AnalyzeExerciseCommandHandler(IAiAnalysisService aiAnalysisService)
    {
        _aiAnalysisService = aiAnalysisService;
    }

    public Task<ExerciseAnalysisDto> Handle(AnalyzeExerciseCommand request, CancellationToken cancellationToken)
    {
        return _aiAnalysisService.AnalyzeAsync(
            request.VideoBytes,
            request.FileName,
            request.ExerciseType,
            cancellationToken);
    }
}
