using MediatR;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;
using StableFit.Domain.Entities;

namespace StableFit.Application.Nutrition.Commands.DeleteNutritionEntry;

public sealed class DeleteNutritionEntryCommandHandler : IRequestHandler<DeleteNutritionEntryCommand>
{
    private readonly INutritionEntryRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public DeleteNutritionEntryCommandHandler(
        INutritionEntryRepository repository,
        IApplicationDbContext dbContext,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteNutritionEntryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var entry = await _repository.GetByIdAsync(request.EntryId, cancellationToken)
            ?? throw new NotFoundException(nameof(NutritionEntry), request.EntryId);

        if (entry.UserId != userId)
            throw new UnauthorizedException();

        await _repository.DeleteAsync(entry, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
