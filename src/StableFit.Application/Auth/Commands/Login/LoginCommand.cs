using MediatR;
using StableFit.Application.Auth.DTOs;

namespace StableFit.Application.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<AuthResultDto>;
