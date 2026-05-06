using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StableFit.Application.Matching.Interfaces;
using StableFit.Application.Matching.Services;

namespace StableFit.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(Behaviors.ValidationBehavior<,>));
        });
        
        services.AddValidatorsFromAssembly(assembly);

        // Matching (Weeks 5-6)
        services.AddSingleton<IMatchScoringService, DefaultMatchScoringService>();

        return services;
    }
}
