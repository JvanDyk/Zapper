using Microsoft.EntityFrameworkCore;
using Zapper.LoyaltyPoints.Domain.Entities.Queue;
using Zapper.LoyaltyPoints.Domain.Interfaces;
using Zapper.LoyaltyPoints.Infrastructure.Persistence;

namespace Zapper.LoyaltyPoints.Infrastructure.Repositories;

public sealed class QueueRepository(LoyaltyDbContext context) : IQueueRepository
{

    public async Task<QueueMessage?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.QueueMessages.FindAsync(id, ct);
    }

    public async Task<IReadOnlyList<QueueMessage>> GetPendingMessagesAsync(int limit = 10, CancellationToken ct = default)
    {
        return await context.QueueMessages
            .Where(m => m.Status == QueueMessageStatus.Pending && m.ScheduledAt <= DateTime.UtcNow)
            .OrderBy(m => m.ScheduledAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<QueueMessage> AddAsync(QueueMessage message, CancellationToken ct = default)
    {
        await context.QueueMessages.AddAsync(message, ct);
        await context.SaveChangesAsync(ct);
        return message;
    }

    public async Task UpdateAsync(QueueMessage message, CancellationToken ct = default)
    {
        context.QueueMessages.Update(message);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var message = await GetByIdAsync(id, ct);
        if (message != null)
        {
            context.QueueMessages.Remove(message);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<QueueHistory> AddHistoryAsync(QueueHistory history, CancellationToken ct = default)
    {
        await context.QueueHistory.AddAsync(history, ct);
        await context.SaveChangesAsync(ct);
        return history;
    }

    public async Task<IReadOnlyList<QueueHistory>> GetHistoryByMessageIdAsync(Guid messageId, CancellationToken ct = default)
    {
        return await context.QueueHistory
            .Where(h => h.QueueMessageId == messageId)
            .OrderByDescending(h => h.ProcessedAt)
            .ToListAsync(ct);
    }

    public async Task<FailedMessage> AddFailedMessageAsync(FailedMessage failedMessage, CancellationToken ct = default)
    {
        await context.FailedMessages.AddAsync(failedMessage, ct);
        await context.SaveChangesAsync(ct);
        return failedMessage;
    }

    public async Task<IReadOnlyList<FailedMessage>> GetRetryableFailedMessagesAsync(CancellationToken ct = default)
    {
        return await context.FailedMessages
            .Where(f => f.CanRetry)
            .OrderBy(f => f.FailedAt)
            .ToListAsync(ct);
    }

    public async Task UpdateFailedMessageAsync(FailedMessage failedMessage, CancellationToken ct = default)
    {
        context.FailedMessages.Update(failedMessage);
        await context.SaveChangesAsync(ct);
    }

    public async Task<int> GetPendingCountAsync(CancellationToken ct = default)
        => await context.QueueMessages.CountAsync(m => m.Status == QueueMessageStatus.Pending, ct);

    public async Task<int> GetProcessingCountAsync(CancellationToken ct = default)
        => await context.QueueMessages.CountAsync(m => m.Status == QueueMessageStatus.Processing, ct);

    public async Task<int> GetDeadLetterCountAsync(CancellationToken ct = default)
        => await context.QueueMessages.CountAsync(m => m.Status == QueueMessageStatus.DeadLetter, ct);

    public async Task<int> GetCompletedCountAsync(CancellationToken ct = default)
        => await context.QueueMessages.CountAsync(m => m.Status == QueueMessageStatus.Completed, ct);
}
