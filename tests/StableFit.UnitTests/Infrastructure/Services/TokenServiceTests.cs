using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using StableFit.Infrastructure.Services;
using StableFit.Infrastructure.Settings;

namespace StableFit.UnitTests.Infrastructure.Services;

public class TokenServiceTests
{
    [Fact]
    public void Constructor_MissingSigningKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SigningKey = "",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        };
        var options = Options.Create(settings);

        // Act & Assert
        var act = () => new TokenService(options);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Jwt:SigningKey is not configured.");
    }

    [Fact]
    public void CreateToken_ValidInputs_ReturnsJwtStringWithCorrectClaims()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SigningKey = "super_secret_key_that_is_at_least_256_bits_long",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        };
        var options = Options.Create(settings);
        var service = new TokenService(options);

        var userId = "user-123";
        var email = "test@example.com";
        var username = "testuser";

        // Act
        var tokenString = service.CreateToken(userId, email, username);

        // Assert
        tokenString.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        token.Issuer.Should().Be(settings.Issuer);
        token.Audiences.Should().Contain(settings.Audience);

        var subClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        subClaim.Should().NotBeNull();
        subClaim!.Value.Should().Be(userId);

        var emailClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be(email);

        var uniqueNameClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName);
        uniqueNameClaim.Should().NotBeNull();
        uniqueNameClaim!.Value.Should().Be(username);
    }
}
