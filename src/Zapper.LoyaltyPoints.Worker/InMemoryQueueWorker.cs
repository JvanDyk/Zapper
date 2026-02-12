using System.Text.Json;
using Zapper.LoyaltyPoints.Application.Responses;
using Zapper.LoyaltyPoints.Application.Interfaces;
using Zapper.LoyaltyPoints.Infrastructure.Messaging;

namespace Zapper.LoyaltyPoints.Worker;

public sealed class InMemoryQueueWorker(
    InMemoryMessageConsumer consumer,
    IServiceScopeFactory scopeFactory,
    ILogger<InMemoryQueueWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("InMemoryQueueWorker started. Waiting for messages...");

        await foreach (var message in consumer.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                logger.LogInformation("Received message from in-memory queue");

                var purchaseEvent = JsonSerializer.Deserialize<PurchaseEventResponse>(message);
                if (purchaseEvent is null)
                {
                    logger.LogWarning("Failed to deserialize message. Skipping.");
                    continue;
                }

                using var scope = scopeFactory.CreateScope();
                var processingService = scope.ServiceProvider.GetRequiredService<IPointsProcessingService>();
                await processingService.ProcessPurchasePointsAsync(purchaseEvent, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message from in-memory queue");
            }
        }
    }
}
