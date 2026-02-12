using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zapper.LoyaltyPoints.Domain.Entities.Queue;
using Zapper.LoyaltyPoints.Infrastructure.Configuration;

namespace Zapper.LoyaltyPoints.Infrastructure.Persistence.DevEmulator;

public sealed class DevEmulatorConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DevEmulatorSettings _settings;
    private readonly ILogger<DevEmulatorConsumer> _logger;

    public DevEmulatorConsumer(
        IServiceScopeFactory scopeFactory,
        AppSettings appSettings,
        ILogger<DevEmulatorConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = appSettings.DevEmulator;
        _logger = logger;
    }

    public async Task<IReadOnlyList<QueueMessage>> ReceiveBatchAsync(CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LoyaltyDbContext>();

        var now = DateTime.UtcNow;
        var visibilityTimeout = TimeSpan.FromSeconds(_settings.VisibilityTimeoutSeconds);

        // Raw SQL with FOR UPDATE SKIP LOCKED for true concurrent consumption
        // Picks up: pending messages whose scheduled_at has passed, OR messages whose lock expired
        var messages = await context.QueueMessages
            .FromSqlInterpolated($@"
                SELECT * FROM queue_messages
                WHERE (status = {(int)QueueMessageStatus.Pending} AND scheduled_at <= {now})
                   OR (status = {(int)QueueMessageStatus.Processing} AND locked_until <= {now})
                ORDER BY scheduled_at ASC
                LIMIT {_settings.BatchSize}
                FOR UPDATE SKIP LOCKED")
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            message.Lock(visibilityTimeout);
        }

        if (messages.Count > 0)
        {
            await context.SaveChangesAsync(ct);
            _logger.LogInformation("[DevEmulator] Received {Count} messages from queue", messages.Count);
        }

        return messages;
    }

    public async Task CompleteAsync(Guid messageId, long elapsedMs, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LoyaltyDbContext>();

        var message = await context.QueueMessages.FindAsync([messageId], ct);
        if (message is null) return;

        message.Complete();

        var history = QueueHistory.FromMessage(message, elapsedMs);
        await context.QueueHistory.AddAsync(history, ct);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("[DevEmulator] Completed message {MessageId} in {ElapsedMs}ms", messageId, elapsedMs);
    }

    public async Task FailAsync(Guid messageId, string error, string? stackTrace, long elapsedMs, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LoyaltyDbContext>();

        var message = await context.QueueMessages.FindAsync([messageId], ct);
        if (message is null) return;

        message.Fail(error, stackTrace);

        var history = QueueHistory.FromMessage(message, elapsedMs);
        await context.QueueHistory.AddAsync(history, ct);
        await context.SaveChangesAsync(ct);

        if (message.Status == QueueMessageStatus.DeadLetter)
        {
            // Create a FailedMessage entry for dead-letter tracking (following example pattern)
            var failedMessage = FailedMessage.FromDeadLetter(message);
            await context.FailedMessages.AddAsync(failedMessage, ct);
            await context.SaveChangesAsync(ct);

            _logger.LogError("[DevEmulator] Message {MessageId} moved to dead-letter after {RetryCount} retries. Error: {Error}",
                messageId, message.RetryCount, error);
        }
        else
        {
            _logger.LogWarning("[DevEmulator] Message {MessageId} failed (attempt {RetryCount}/{MaxRetries}). Retrying. Error: {Error}",
                messageId, message.RetryCount, _settings.MaxRetries, error);
        }
    }

    public async Task ReleaseOrphanedMessagesAsync(CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LoyaltyDbContext>();

        var now = DateTime.UtcNow;
        var orphaned = await context.QueueMessages
            .Where(m => m.Status == QueueMessageStatus.Processing && m.LockedUntil <= now)
            .ToListAsync(ct);

        foreach (var message in orphaned)
        {
            message.Status = QueueMessageStatus.Pending;
            message.LockedUntil = null;
            message.ScheduledAt = now;
        }

        if (orphaned.Count > 0)
        {
            await context.SaveChangesAsync(ct);
            _logger.LogWarning("[DevEmulator] Released {Count} orphaned messages back to queue", orphaned.Count);
        }
    }
}
