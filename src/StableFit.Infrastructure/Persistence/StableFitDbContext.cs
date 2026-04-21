using System.Reflection;
using Microsoft.EntityFrameworkCore;
using StableFit.Application.Interfaces;
using StableFit.Domain.Entities;

namespace StableFit.Infrastructure.Persistence;

public sealed class StableFitDbContext : DbContext, IApplicationDbContext
{
    public StableFitDbContext(DbContextOptions<StableFitDbContext> options) : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // This automatically discovers and applies all IEntityTypeConfiguration classes in the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
