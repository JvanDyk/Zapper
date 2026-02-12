namespace Zapper.LoyaltyPoints.Domain.Interfaces;

public interface IQueueRepository
{
    Task<QueueMessage?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<QueueMessage>> GetPendingMessagesAsync(int limit = 10, CancellationToken ct = default);
    Task<QueueMessage> AddAsync(QueueMessage message, CancellationToken ct = default);
    Task UpdateAsync(QueueMessage message, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<QueueHistory> AddHistoryAsync(QueueHistory history, CancellationToken ct = default);
    Task<IReadOnlyList<QueueHistory>> GetHistoryByMessageIdAsync(Guid messageId, CancellationToken ct = default);
    Task<FailedMessage> AddFailedMessageAsync(FailedMessage failedMessage, CancellationToken ct = default);
    Task<IReadOnlyList<FailedMessage>> GetRetryableFailedMessagesAsync(CancellationToken ct = default);
    Task UpdateFailedMessageAsync(FailedMessage failedMessage, CancellationToken ct = default);
    Task<int> GetPendingCountAsync(CancellationToken ct = default);
    Task<int> GetProcessingCountAsync(CancellationToken ct = default);
    Task<int> GetDeadLetterCountAsync(CancellationToken ct = default);
    Task<int> GetCompletedCountAsync(CancellationToken ct = default);
}
