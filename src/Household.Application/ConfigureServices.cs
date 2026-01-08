using System.Reflection;
using FluentValidation;
using Household.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Household.Shared.Services.Interfaces;
using Household.Shared.Services;

//namespace Household.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        //services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
        });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

            config.AddOpenBehavior(typeof(LoggingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        // Register a limiter for the TV API with max concurrency = 5
        services.AddSingleton<IApiRateLimiter>(new ApiRateLimiter(5));

        //// Register pipeline behavior
        //services.AddTransient(
        //    typeof(IPipelineBehavior<,>),
        //    typeof(IConcurrencyLimiting<,>)
        //);

        return services;
    }
}
