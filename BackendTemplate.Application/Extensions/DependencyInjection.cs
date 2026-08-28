using System.Reflection;
using BackendTemplate.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTemplate.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Automatically register all FluentValidation validators in this assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // MediatR with Enterprise Pipeline Behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // Order of pipeline execution:
            // 1. Catch unhandled exceptions
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
            // 2. Log entry, parameters and completion time
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            // 3. Monitor performance and log slow queries/commands (>500ms)
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            // 4. Validate input models with FluentValidation
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
