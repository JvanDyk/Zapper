namespace Zapper.LoyaltyPoints.Infrastructure.Configuration;

public sealed class DevEmulatorSettings
{
    public const string SectionName = "DevEmulator";

    public int PollingIntervalMs { get; init; }
    public int MaxConcurrency { get; init; }
    public int VisibilityTimeoutSeconds { get; init; }
    public int MaxRetries { get; init; }
    public int BatchSize { get; init; }
}
