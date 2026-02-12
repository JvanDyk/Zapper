namespace Zapper.LoyaltyPoints.Domain.Entities.Queue;

public sealed class FailedMessage
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OriginalMessageId { get; private set; }

    public string MessageType { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public string? LastErrorMessage { get; private set; }

    public string? LastStackTrace { get; private set; }

    public int TotalAttempts { get; private set; }

    public bool CanRetry { get; private set; }

    public DateTime FailedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? RetriedAt { get; private set; }

    private FailedMessage() { } // EF Core

    public static FailedMessage FromDeadLetter(QueueMessage message)
    {
        return new FailedMessage
        {
            OriginalMessageId = message.Id,
            MessageType = message.MessageType,
            Payload = message.Payload,
            LastErrorMessage = message.ErrorMessage,
            LastStackTrace = message.ErrorStackTrace,
            TotalAttempts = message.RetryCount,
            CanRetry = true
        };
    }

    public void MarkRetried()
    {
        CanRetry = false;
        RetriedAt = DateTime.UtcNow;
    }
}
