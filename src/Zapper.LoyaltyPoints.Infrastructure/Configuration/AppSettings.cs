namespace Zapper.LoyaltyPoints.Infrastructure.Configuration;

public sealed class AppSettings
{
    public ConnectionStrings ConnectionStrings { get; init; }
    public MessagingSettings Messaging { get; init; }
    public DevEmulatorSettings DevEmulator { get; init; }
    public AwsSettings Aws { get; init; }
    public SerilogSettings Serilog { get; init; }
    public PointsCalculationSettings PointsCalculation { get; init; }
    public string AllowedHosts { get; init; }
}
