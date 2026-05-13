using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StableFit.Infrastructure.Persistence;

/// <summary>
/// Used exclusively by EF Core tools (migrations) at design time.
/// Connection details are read from environment variables so that no credentials
/// are stored in source code. Set the variables in your shell or .env file before
/// running 'dotnet ef' commands.
/// </summary>
public sealed class StableFitDbContextFactory : IDesignTimeDbContextFactory<StableFitDbContext>
{
    public StableFitDbContext CreateDbContext(string[] args)
    {
        var host     = Environment.GetEnvironmentVariable("POSTGRES_HOST")     ?? "localhost";
        var port     = Environment.GetEnvironmentVariable("POSTGRES_PORT")     ?? "5432";
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB")       ?? "stablefit";
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER")     ?? "stablefit";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? string.Empty;

        var connectionString =
            $"Host={host};Port={port};Database={database};Username={username};Password={password}";

        var optionsBuilder = new DbContextOptionsBuilder<StableFitDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3));

        return new StableFitDbContext(optionsBuilder.Options);
    }
}
