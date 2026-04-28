using MediatR;
using StableFit.Application.Auth.DTOs;

namespace StableFit.Application.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string Username) : IRequest<AuthResultDto>;
