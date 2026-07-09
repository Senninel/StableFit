using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StableFit.Application.Exercise.Commands.AnalyzeExercise;

namespace StableFit.API.Controllers;

[ApiController]
[Route("api/exercise")]
[Authorize]
public sealed class ExerciseController : ControllerBase
{
    private readonly ISender _sender;

    public ExerciseController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("analyze")]
    [RequestSizeLimit(100 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
    public async Task<IActionResult> Analyze(
        IFormFile file,
        [FromForm] string exerciseType,
        CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);

        var command = new AnalyzeExerciseCommand
        {
            VideoBytes = ms.ToArray(),
            FileName = file.FileName,
            ExerciseType = exerciseType
        };

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }
}
