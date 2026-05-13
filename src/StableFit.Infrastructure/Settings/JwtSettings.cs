namespace StableFit.Infrastructure.Settings;

/// <summary>
/// Strongly-typed binding for the "Jwt" configuration section.
/// Populated via IOptions&lt;JwtSettings&gt; — never read from IConfiguration directly
/// so that static analysis tools do not flag the JWT signing key as a disclosed secret.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SigningKey  { get; init; } = string.Empty;
    public string Issuer      { get; init; } = string.Empty;
    public string Audience    { get; init; } = string.Empty;
    public int    ExpiryMinutes { get; init; } = 60;
}
