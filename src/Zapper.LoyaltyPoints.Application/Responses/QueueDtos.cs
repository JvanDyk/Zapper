using System.Text.Json.Serialization;

namespace Zapper.LoyaltyPoints.Application.Responses;

public sealed record QueueStatusResponse
{
    [JsonPropertyName("pending")]
    public int Pending { get; init; }

    [JsonPropertyName("processing")]
    public int Processing { get; init; }

    [JsonPropertyName("dead_letter")]
    public int DeadLetter { get; init; }

    [JsonPropertyName("completed")]
    public int Completed { get; init; }

    [JsonPropertyName("total")]
    public int Total { get; init; }
}

public sealed record FailedMessageResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("original_message_id")]
    public Guid OriginalMessageId { get; init; }

    [JsonPropertyName("message_type")]
    public string MessageType { get; init; } = default!;

    [JsonPropertyName("payload")]
    public string Payload { get; init; } = default!;

    [JsonPropertyName("last_error")]
    public string? LastError { get; init; }

    [JsonPropertyName("total_attempts")]
    public int TotalAttempts { get; init; }

    [JsonPropertyName("can_retry")]
    public bool CanRetry { get; init; }

    [JsonPropertyName("failed_at")]
    public DateTime FailedAt { get; init; }

    [JsonPropertyName("retried_at")]
    public DateTime? RetriedAt { get; init; }
}

public sealed record QueueHistoryEntryResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("queue_message_id")]
    public Guid QueueMessageId { get; init; }

    [JsonPropertyName("message_type")]
    public string MessageType { get; init; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; init; } = default!;

    [JsonPropertyName("processed_at")]
    public DateTime ProcessedAt { get; init; }

    [JsonPropertyName("elapsed_ms")]
    public long ElapsedMs { get; init; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; init; }

    [JsonPropertyName("attempt_number")]
    public int AttemptNumber { get; init; }
}

public sealed record QueueRetryResultResponse
{
    [JsonPropertyName("message")]
    public string Message { get; init; } = default!;

    [JsonPropertyName("new_message_id")]
    public Guid NewMessageId { get; init; }

    [JsonPropertyName("original_failed_id")]
    public Guid OriginalFailedId { get; init; }
}

public sealed record QueueUnavailableDto
{
    [JsonPropertyName("message")]
    public string Message { get; init; } = "Queue monitoring is only available when using the DevEmulator provider.";
}
