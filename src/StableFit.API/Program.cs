using Microsoft.EntityFrameworkCore;
using Npgsql;
using StableFit.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Swagger (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

static string? GetEnv(string key)
    => Environment.GetEnvironmentVariable(key);

var configuredConnectionString = builder.Configuration.GetConnectionString("StableFit");

// Prefer environment-based config so Docker/.env and local runs stay consistent.
var host = GetEnv("POSTGRES_HOST");
var portRaw = GetEnv("POSTGRES_PORT");
var dbName = GetEnv("POSTGRES_DB");
var user = GetEnv("POSTGRES_USER");
var password = GetEnv("POSTGRES_PASSWORD");

var hasEnvDbConfig = !string.IsNullOrWhiteSpace(host)
                     && !string.IsNullOrWhiteSpace(portRaw)
                     && !string.IsNullOrWhiteSpace(dbName)
                     && !string.IsNullOrWhiteSpace(user)
                     && !string.IsNullOrWhiteSpace(password);

string connectionString;

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

    connectionString = csb.ConnectionString;
}
else if (!string.IsNullOrWhiteSpace(configuredConnectionString))
{
    connectionString = configuredConnectionString;
}
else
{
    throw new InvalidOperationException(
        "Database is not configured. Set POSTGRES_HOST/PORT/DB/USER/PASSWORD env vars or configure ConnectionStrings:StableFit.");
}

builder.Services.AddDbContext<StableFitDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "StableFit API v1");
        options.DisplayRequestDuration();
    });

    // Auto-apply migrations in Development so the DB schema stays in sync.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<StableFitDbContext>();
    await db.Database.MigrateAsync();
}
else
{
    app.UseHttpsRedirection();
}

app.MapGet("/db/health", async (StableFitDbContext db, CancellationToken ct) =>
{
    var canConnect = await db.Database.CanConnectAsync(ct);
    return Results.Ok(new { canConnect });
});

app.MapGet("/db/migrations", async (StableFitDbContext db, CancellationToken ct) =>
{
    var applied = await db.Database.GetAppliedMigrationsAsync(ct);
    return Results.Ok(applied);
});

app.Run();
