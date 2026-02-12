using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MediatR;
using Zapper.LoyaltyPoints.Application.Commands;
using Zapper.LoyaltyPoints.Application.Responses;
using Zapper.LoyaltyPoints.Domain.Entities.Queue;
using Zapper.LoyaltyPoints.Infrastructure.Persistence.DevEmulator;
using Zapper.LoyaltyPoints.Infrastructure.Configuration;

namespace Zapper.LoyaltyPoints.Worker;

public sealed class SemaphoreSlimQueueWorker(
    DevEmulatorConsumer consumer,
    IServiceScopeFactory scopeFactory,
    IOptions<DevEmulatorSettings> settings,
    ILogger<SemaphoreSlimQueueWorker> logger) : BackgroundService
{
    private readonly DevEmulatorSettings _settings = settings.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[SemaphoreSlimQueueWorker] ExecuteAsync entered");
        
        logger.LogInformation(
            "SemaphoreSlimQueueWorker started. Polling every {IntervalMs}ms, max concurrency={MaxConcurrency}, batch={BatchSize}",
            _settings.PollingIntervalMs, _settings.MaxConcurrency, _settings.BatchSize);
        
        await Task.Yield(); // Ensure the host can continue starting
        
        await ReleaseOrphanedWithRetryAsync(stoppingToken);

        using var semaphore = new SemaphoreSlim(_settings.MaxConcurrency);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogDebug("[SemaphoreSlimQueueWorker] Polling for messages...");
                var messages = await consumer.ReceiveBatchAsync(stoppingToken);

                if (messages.Count == 0)
                {
                    await Task.Delay(_settings.PollingIntervalMs, stoppingToken);
                    continue;
                }

                logger.LogInformation("[SemaphoreSlimQueueWorker] Processing {Count} messages", messages.Count);
                var processingTasks = messages.Select(message => ProcessMessageAsync(message, semaphore, stoppingToken)).ToList();
                await Task.WhenAll(processingTasks);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in SemaphoreSlimQueueWorker polling loop");
                await Task.Delay(_settings.PollingIntervalMs * 2, stoppingToken);
            }
        }

        logger.LogInformation("SemaphoreSlimQueueWorker stopped.");
    }

    private async Task ReleaseOrphanedWithRetryAsync(CancellationToken stoppingToken)
    {
        const int maxRetries = 10;
        const int baseDelayMs = 3000;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await consumer.ReleaseOrphanedMessagesAsync(stoppingToken);
                logger.LogInformation("Orphaned messages released successfully");
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "ReleaseOrphanedMessages attempt {Attempt}/{MaxRetries} failed. Retrying in {DelayMs}ms...",
                    attempt, maxRetries, baseDelayMs * attempt);

                if (attempt == maxRetries)
                {
                    logger.LogError("ReleaseOrphanedMessages failed after {MaxRetries} attempts. Continuing without cleanup.", maxRetries);
                    return;
                }

                await Task.Delay(baseDelayMs * attempt, stoppingToken);
            }
        }
    }

    private async Task ProcessMessageAsync(QueueMessage message, SemaphoreSlim semaphore, CancellationToken stoppingToken)
    {
        await semaphore.WaitAsync(stoppingToken);

        try
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var purchaseEvent = JsonSerializer.Deserialize<PurchaseEventResponse>(message.Payload);
                if (purchaseEvent is null)
                {
                    logger.LogWarning("Failed to deserialize message {MessageId}. Completing as dead-letter.", message.Id);
                    await consumer.FailAsync(message.Id, "Deserialization failed", null, stopwatch.ElapsedMilliseconds, stoppingToken);
                    return;
                }

                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new ProcessPointsCommand(purchaseEvent), stoppingToken);

                stopwatch.Stop();
                await consumer.CompleteAsync(message.Id, stopwatch.ElapsedMilliseconds, stoppingToken);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Error processing message {MessageId}", message.Id);
                await consumer.FailAsync(message.Id, ex.Message, ex.StackTrace, stopwatch.ElapsedMilliseconds, stoppingToken);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
}
