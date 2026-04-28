using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StableFit.Application.Auth.Commands.Login;
using StableFit.Application.Auth.Commands.Register;
using StableFit.Application.Auth.DTOs;

namespace StableFit.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IConfiguration _configuration;

    public AuthController(ISender sender, IConfiguration configuration)
    {
        _sender = sender;
        _configuration = configuration;
    }

    /// <summary>Registers a new user and sets an auth cookie.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.Username);
        var result = await _sender.Send(command, ct);

        SetAuthCookie(result.AccessToken);

        return Created("/api/auth/me", new { result.UserId, result.Email, result.Username });
    }

    /// <summary>Logs in and sets an auth cookie.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _sender.Send(command, ct);

        SetAuthCookie(result.AccessToken);

        return Ok(new { result.UserId, result.Email, result.Username });
    }

    /// <summary>Clears the auth cookie.</summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("sf_access_token");
        return NoContent();
    }

    /// <summary>Returns the current authenticated user's claims. Useful for debugging.</summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email);
        var username = User.FindFirstValue(JwtRegisteredClaimNames.UniqueName);

        return Ok(new { userId, email, username });
    }

    private void SetAuthCookie(string token)
    {
        var expiryMinutes = GetExpiryMinutes();

        Response.Cookies.Append("sf_access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,       // Secure only over HTTPS; dev HTTP is fine
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
        });
    }

    private int GetExpiryMinutes()
    {
        const int defaultMinutes = 60;
        return int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var m) ? m : defaultMinutes;
    }
}
