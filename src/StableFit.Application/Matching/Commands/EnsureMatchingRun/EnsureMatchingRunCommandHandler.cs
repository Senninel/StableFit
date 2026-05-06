using MediatR;
using StableFit.Application.Matching.Interfaces;

namespace StableFit.Application.Matching.Commands.EnsureMatchingRun;

public sealed class EnsureMatchingRunCommandHandler : IRequestHandler<EnsureMatchingRunCommand, Guid>
{
    private readonly IMatchingRunBuilder _builder;

    public EnsureMatchingRunCommandHandler(IMatchingRunBuilder builder)
    {
        _builder = builder;
    }

    public Task<Guid> Handle(EnsureMatchingRunCommand request, CancellationToken cancellationToken)
        => _builder.EnsureRunAndMaterializeRecommendationsAsync(cancellationToken);
}
