namespace Zapper.LoyaltyPoints.Domain.Entities.Queue;

public sealed class QueueMessage
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string MessageType { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public QueueMessageStatus Status { get; set; }

    public int RetryCount { get; private set; }

    public int MaxRetries { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ScheduledAt { get; set; }

    public DateTime? LockedUntil { get; set; }

    public DateTime? ProcessedAt { get; private set; }

    public string? ErrorMessage { get; set; }

    public string? ErrorStackTrace { get; set; }

    private QueueMessage() { } // EF Core

    public QueueMessage(string messageType, string payload, int maxRetries = 3)
    {
        MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        MaxRetries = maxRetries;
        Status = QueueMessageStatus.Pending;
        ScheduledAt = DateTime.UtcNow;
    }

    public void Lock(TimeSpan visibilityTimeout)
    {
        Status = QueueMessageStatus.Processing;
        LockedUntil = DateTime.UtcNow.Add(visibilityTimeout);
    }

    public void MarkAsProcessing()
    {
        Status = QueueMessageStatus.Processing;
    }

    public void Complete()
    {
        Status = QueueMessageStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
        LockedUntil = null;
    }

    public void Fail(string error, string? stackTrace = null)
    {
        RetryCount++;
        ErrorMessage = error;
        ErrorStackTrace = stackTrace;
        LockedUntil = null;

        if (RetryCount >= MaxRetries)
        {
            Status = QueueMessageStatus.DeadLetter;
        }
        else
        {
            Status = QueueMessageStatus.Pending;
            // Exponential backoff: 2^retryCount seconds (2s, 4s, 8s, ...)
            ScheduledAt = DateTime.UtcNow.AddSeconds(Math.Pow(2, RetryCount));
        }
    }

    public bool CanRetry => RetryCount < MaxRetries;
    public bool ShouldRetry() => CanRetry && Status == QueueMessageStatus.Pending;
}

public enum QueueMessageStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    DeadLetter = 3
}
