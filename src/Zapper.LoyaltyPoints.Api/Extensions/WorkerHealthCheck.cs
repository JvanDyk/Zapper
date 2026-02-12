using Microsoft.Extensions.Diagnostics.HealthChecks;
using Zapper.LoyaltyPoints.Infrastructure.Configuration;

namespace Zapper.LoyaltyPoints.Api.Extensions;

public sealed class WorkerHealthCheck(
    IServiceProvider serviceProvider,
    AppSettings appSettings) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var messagingSettings = appSettings.Messaging;
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        var workerRunning = hostedServices.Any();

        if (!workerRunning)
        {
            return Task.FromResult(HealthCheckResult.Healthy(
                $"Worker expected as separate process. Messaging provider: {messagingSettings.Provider}"));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Worker hosted service registered. Provider: {messagingSettings.Provider}"));
    }
}
