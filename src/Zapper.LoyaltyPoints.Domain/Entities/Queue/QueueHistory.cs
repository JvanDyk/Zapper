namespace Zapper.LoyaltyPoints.Domain.Entities.Queue;

public sealed class QueueHistory
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid QueueMessageId { get; private set; }

    public string MessageType { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public QueueMessageStatus Status { get; private set; }

    public DateTime ProcessedAt { get; private set; } = DateTime.UtcNow;
    public long ElapsedMs { get; private set; }

    public string? ErrorMessage { get; private set; }

    public int AttemptNumber { get; private set; }

    private QueueHistory() { } // EF Core

    public static QueueHistory FromMessage(QueueMessage message, long elapsedMs)
    {
        return new QueueHistory
        {
            QueueMessageId = message.Id,
            MessageType = message.MessageType,
            Payload = message.Payload,
            Status = message.Status,
            ElapsedMs = elapsedMs,
            ErrorMessage = message.ErrorMessage,
            AttemptNumber = message.RetryCount
        };
    }
}
