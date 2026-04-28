using MediatR;
using StableFit.Application.Auth.DTOs;
using StableFit.Application.Interfaces;

namespace StableFit.Application.Auth.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(IIdentityService identityService, ITokenService tokenService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, userId, errors) = await _identityService.RegisterAsync(
            request.Email, request.Password, request.Username, cancellationToken);

        if (!succeeded || userId is null)
        {
            var detail = string.Join("; ", errors);
            throw new InvalidOperationException(detail);
        }

        var token = _tokenService.CreateToken(userId, request.Email, request.Username);

        return new AuthResultDto(userId, request.Email, request.Username, token);
    }
}
