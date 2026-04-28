namespace StableFit.Application.Interfaces;

/// <summary>
/// Creates JWT access tokens. Implemented in Infrastructure.
/// </summary>
public interface ITokenService
{
    string CreateToken(string userId, string email, string username);
}
