using MediatR;
using Microsoft.EntityFrameworkCore;
using StableFit.Application.Exceptions;
using StableFit.Application.Interfaces;
using StableFit.Application.Matching.DTOs;
using StableFit.Application.Matching.Queries.GetMyActiveMatch;
using StableFit.Domain.Enums;
using StableFit.Infrastructure.Persistence;

namespace StableFit.Infrastructure.Services.Matching;

public sealed class GetMyActiveMatchQueryHandler : IRequestHandler<GetMyActiveMatchQuery, ActiveMatchDto?>
{
    private readonly StableFitDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyActiveMatchQueryHandler(StableFitDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ActiveMatchDto?> Handle(GetMyActiveMatchQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        // Find the active match where the user is either User1 or User2
        var match = await _db.Matches
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Status == MatchStatus.Active &&
                                      (m.UserId1 == userId || m.UserId2 == userId),
                cancellationToken);

        if (match is null)
            return null;

        var partnerUserId = match.UserId1 == userId ? match.UserId2 : match.UserId1;

        // Get partner profile
        var partnerProfile = await _db.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == partnerUserId, cancellationToken);

        if (partnerProfile is null)
            return null; // Should not happen in a consistent DB

        return new ActiveMatchDto(
            match.Id,
            match.CreatedAtUtc,
            partnerProfile.UserId,
            partnerProfile.Username,
            partnerProfile.Name,
            partnerProfile.Bio,
            partnerProfile.Goal,
            partnerProfile.ScheduleDays,
            partnerProfile.AgeYears,
            partnerProfile.WeightKg
        );
    }
}
