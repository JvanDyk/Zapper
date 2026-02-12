namespace Zapper.LoyaltyPoints.Infrastructure.Configuration;

public sealed class SerilogMinimumLevel
{
    public string Default { get; init; }
    public Dictionary<string, string> Override { get; init; }
}
