using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.Results;
using StableFit.Application.Exercise.DTOs;
using StableFit.Application.Interfaces;

namespace StableFit.Infrastructure.Services;

public sealed class AiAnalysisService : IAiAnalysisService
{
    private readonly HttpClient _httpClient;

    public AiAnalysisService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExerciseAnalysisDto> AnalyzeAsync(
        byte[] videoBytes,
        string fileName,
        string exerciseType,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(videoBytes), "file", fileName);
        content.Add(new StringContent(exerciseType), "exercise_type");

        var response = await _httpClient.PostAsync("/analyze-exercise", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ValidationException(
            [
                new ValidationFailure("Video", $"AI service error: {detail}")
            ]);
        }

        var aiResponse = await response.Content.ReadFromJsonAsync<AiServiceResponse>(cancellationToken)
            ?? throw new InvalidOperationException("AI service returned an empty response.");

        return new ExerciseAnalysisDto
        {
            FrameCount = aiResponse.FrameCount,
            Fps = aiResponse.Fps,
            ExerciseType = aiResponse.ExerciseType,
            Score = aiResponse.Score,
            RepCount = aiResponse.RepCount,
            Feedback = aiResponse.Feedback
        };
    }

    private sealed record AiServiceResponse(
        [property: JsonPropertyName("frame_count")] int FrameCount,
        [property: JsonPropertyName("fps")] double Fps,
        [property: JsonPropertyName("exercise_type")] string ExerciseType,
        [property: JsonPropertyName("score")] int Score,
        [property: JsonPropertyName("rep_count")] int RepCount,
        [property: JsonPropertyName("feedback")] IReadOnlyList<string> Feedback
    );
}
