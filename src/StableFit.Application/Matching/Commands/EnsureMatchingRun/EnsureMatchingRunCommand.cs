using MediatR;

namespace StableFit.Application.Matching.Commands.EnsureMatchingRun;

public sealed record EnsureMatchingRunCommand : IRequest<Guid>;

