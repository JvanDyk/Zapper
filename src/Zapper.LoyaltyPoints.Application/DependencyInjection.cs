using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Zapper.LoyaltyPoints.Application.Behaviors;

namespace Zapper.LoyaltyPoints.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string messagingProvider = "")
    {
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });
        
        services.AddValidatorsFromAssembly(assembly);

        // Unregister queue handlers if not using DevEmulator
        if (messagingProvider != "DevEmulator")
        {
            UnregisterQueueHandlers(services);
        }

        return services;
    }

    private static void UnregisterQueueHandlers(IServiceCollection services)
    {
        var queueHandlerTypes = new[]
        {
            "Zapper.LoyaltyPoints.Application.Queries.GetQueueStatusQueryHandler",
            "Zapper.LoyaltyPoints.Application.Queries.GetDeadLettersQueryHandler",
            "Zapper.LoyaltyPoints.Application.Queries.GetQueueHistoryQueryHandler"
        };

        foreach (var handlerTypeName in queueHandlerTypes)
        {
            var descriptor = services.FirstOrDefault(d =>
                d.ImplementationType?.FullName == handlerTypeName);
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
        }
    }
}
