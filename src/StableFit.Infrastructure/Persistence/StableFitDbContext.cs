using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StableFit.Application.Interfaces;
using StableFit.Domain.Entities;
using StableFit.Infrastructure.Identity;

namespace StableFit.Infrastructure.Persistence;

public sealed class StableFitDbContext : IdentityDbContext<AppUser>, IApplicationDbContext
{
    public StableFitDbContext(DbContextOptions<StableFitDbContext> options) : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Auto-discovers and applies all IEntityTypeConfiguration classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
