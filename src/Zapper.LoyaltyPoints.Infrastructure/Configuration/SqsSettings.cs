namespace Zapper.LoyaltyPoints.Infrastructure.Configuration;

public sealed class SqsSettings
{
    public string QueueName { get; init; }
    public string Region { get; init; }
    public int MaxNumberOfMessages { get; init; }
    public int WaitTimeSeconds { get; init; }
    public int VisibilityTimeoutSeconds { get; init; }
}
