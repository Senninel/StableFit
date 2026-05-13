using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StableFit.Infrastructure.Settings;

namespace StableFit.Infrastructure.Services;

/// <summary>
/// Configures <see cref="JwtBearerOptions"/> from <see cref="JwtSettings"/> at startup.
/// Using IConfigureNamedOptions keeps the JWT signing key out of the DI registration
/// lambda, satisfying SonarQube S6781 (no direct IConfiguration access to a sensitive key).
/// </summary>
internal sealed class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtSettings _jwtSettings;

    public JwtBearerOptionsSetup(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public void Configure(JwtBearerOptions options) => Configure(Options.DefaultName, options);

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.SigningKey))
            throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.SigningKey);

        options.TokenValidationParameters.ValidIssuer       = _jwtSettings.Issuer;
        options.TokenValidationParameters.ValidAudience     = _jwtSettings.Audience;
        options.TokenValidationParameters.IssuerSigningKey  = new SymmetricSecurityKey(keyBytes);
    }
}
