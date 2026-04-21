using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using StableFit.Application.Interfaces;
using StableFit.Infrastructure.Persistence;
using StableFit.Infrastructure.Persistence.Repositories;

namespace StableFit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetConnectionString(configuration);

        services.AddDbContext<StableFitDbContext>(options => 
            options.UseNpgsql(connectionString, npgsql => 
            {
                npgsql.EnableRetryOnFailure(maxRetryCount: 3);
            }));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<StableFitDbContext>());
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        return services;
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        var configuredConnectionString = configuration.GetConnectionString("StableFit");

        var host = configuration["POSTGRES_HOST"];
        var portRaw = configuration["POSTGRES_PORT"];
        var dbName = configuration["POSTGRES_DB"];
        var user = configuration["POSTGRES_USER"];
        var password = configuration["POSTGRES_PASSWORD"];

        var hasEnvDbConfig = !string.IsNullOrWhiteSpace(host)
                             && !string.IsNullOrWhiteSpace(portRaw)
                             && !string.IsNullOrWhiteSpace(dbName)
                             && !string.IsNullOrWhiteSpace(user)
                             && !string.IsNullOrWhiteSpace(password);

        if (hasEnvDbConfig)
        {
            _ = int.TryParse(portRaw, out var port);

            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = port == 0 ? 5432 : port,
                Database = dbName,
                Username = user,
                Password = password
            };

            return csb.ConnectionString;
        }

        if (!string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            return configuredConnectionString;
        }

        throw new InvalidOperationException(
            "Database is not configured. Set POSTGRES_HOST/PORT/DB/USER/PASSWORD env vars or configure ConnectionStrings:StableFit.");
    }
}
