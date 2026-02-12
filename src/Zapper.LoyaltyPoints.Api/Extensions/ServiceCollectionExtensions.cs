using FluentValidation.AspNetCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Zapper.LoyaltyPoints.Infrastructure.Persistence;

namespace Zapper.LoyaltyPoints.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        services.AddControllers();
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddOpenApi();

        var healthChecks = services.AddHealthChecks()
            .AddDbContextCheck<LoyaltyDbContext>("database", tags: ["ready"])
            .AddCheck("live", () => HealthCheckResult.Healthy("Service is alive"), tags: ["live"]);

        if (hostEnvironment.IsDevelopment())
        {
            healthChecks.AddCheck<WorkerHealthCheck>("worker", tags: ["ready"]);
        }
        else
        {
            healthChecks.AddCheck<MessagingHealthCheck>("messaging", tags: ["ready"]);
        }

        return services;
    }
}
