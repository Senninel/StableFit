namespace StableFit.Application.Auth.DTOs;

public sealed record RegisterRequest(string Email, string Password, string Username);

public sealed record LoginRequest(string Email, string Password);
