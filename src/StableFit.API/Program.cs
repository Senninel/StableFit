using Microsoft.EntityFrameworkCore;
using StableFit.Application;
using StableFit.Application.Commands.UserProfiles.CreateUserProfile;
using StableFit.Application.DTOs.UserProfiles;
using StableFit.Application.Queries.UserProfiles.GetUserProfileById;
using StableFit.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Swagger (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Application layer (MediatR, FluentValidation, etc.)
builder.Services.AddApplication();

// Infrastructure (DbContext + repositories)
builder.Services.AddInfrastructure(builder.Configuration);

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

app.MapControllers();

await app.RunAsync();
