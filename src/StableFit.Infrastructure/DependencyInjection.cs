using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using StableFit.Application.Interfaces;
using StableFit.Infrastructure.Identity;
using StableFit.Infrastructure.Persistence;
using StableFit.Infrastructure.Persistence.Repositories;
using StableFit.Infrastructure.Services;

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

        services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<StableFitDbContext>();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();

        AddJwtAuthentication(services, configuration);

        return services;
    }

    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Prevent .NET 8+ from rewriting our claims (sub -> NameIdentifier)
                options.MapInboundClaims = false;
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = key,
                    // Use short claim names (sub, email) instead of ClaimTypes URIs
                    NameClaimType = "unique_name",
                    RoleClaimType = "role"
                };

                // Read token from HttpOnly cookie instead of Authorization header
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["sf_access_token"];
                        return Task.CompletedTask;
                    }
                };
            });
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
            return configuredConnectionString;

        throw new InvalidOperationException(
            "Database is not configured. Set POSTGRES_HOST/PORT/DB/USER/PASSWORD env vars or configure ConnectionStrings:StableFit.");
    }
}
