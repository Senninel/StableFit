namespace StableFit.Application.Auth.DTOs;

public sealed record AuthResultDto(
    string UserId,
    string Email,
    string Username,
    string AccessToken);
