using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StableFit.Infrastructure.Persistence;

/// <summary>
/// Used exclusively by EF Core tools (migrations) at design time.
/// The connection string here is for local dev only — never used at runtime.
/// </summary>
public sealed class StableFitDbContextFactory : IDesignTimeDbContextFactory<StableFitDbContext>
{
    public StableFitDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<StableFitDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=stablefit;Username=postgres;Password=postgres",
            npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3));

        return new StableFitDbContext(optionsBuilder.Options);
    }
}
