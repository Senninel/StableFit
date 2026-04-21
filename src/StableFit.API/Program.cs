using Microsoft.EntityFrameworkCore;
using StableFit.API.Infrastructure;
using StableFit.Application;
using StableFit.Infrastructure;
using StableFit.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Swagger (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Application layer (MediatR, FluentValidation, etc.)
builder.Services.AddApplication();

// Infrastructure (DbContext + repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Global Exception Handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

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

app.MapControllers();

await app.RunAsync();
