using MediatR;
using StableFit.Application.Auth.DTOs;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;

namespace StableFit.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IIdentityService identityService, ITokenService tokenService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, userId, email, username) = await _identityService.LoginAsync(
            request.Email, request.Password, cancellationToken);

        if (!succeeded || userId is null || email is null || username is null)
            throw new UnauthorizedException("Invalid email or password.");

        var token = _tokenService.CreateToken(userId, email, username);

        return new AuthResultDto(userId, email, username, token);
    }
}
