using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}
