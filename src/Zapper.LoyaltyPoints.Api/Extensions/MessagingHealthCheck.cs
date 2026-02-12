using Microsoft.Extensions.Diagnostics.HealthChecks;
using Zapper.LoyaltyPoints.Domain.Interfaces;

namespace Zapper.LoyaltyPoints.Api.Extensions;

public sealed class MessagingHealthCheck(IServiceProvider serviceProvider) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var publisher = scope.ServiceProvider.GetService<IMessagePublisher>();

            return publisher is not null
                ? Task.FromResult(HealthCheckResult.Healthy($"Messaging publisher registered: {publisher.GetType().Name}"))
                : Task.FromResult(HealthCheckResult.Unhealthy("IMessagePublisher is not registered in the DI container."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Failed to resolve messaging publisher.", ex));
        }
    }
}
