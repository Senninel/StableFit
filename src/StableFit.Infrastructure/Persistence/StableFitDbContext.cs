using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StableFit.Application.Interfaces;
using StableFit.Domain.Entities;
using StableFit.Infrastructure.Identity;
using StableFit.Infrastructure.Persistence.Entities;

namespace StableFit.Infrastructure.Persistence;

public sealed class StableFitDbContext : IdentityDbContext<AppUser>, IApplicationDbContext
{
    public StableFitDbContext(DbContextOptions<StableFitDbContext> options) : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    // Matching (Weeks 5-6)
    public DbSet<MatchRun> MatchRuns => Set<MatchRun>();
    public DbSet<MatchRecommendation> MatchRecommendations => Set<MatchRecommendation>();
    public DbSet<MatchDecision> MatchDecisions => Set<MatchDecision>();
    public DbSet<Match> Matches => Set<Match>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Auto-discovers and applies all IEntityTypeConfiguration classes in this assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
