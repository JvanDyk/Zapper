using Zapper.LoyaltyPoints.Infrastructure.Models.Enums;

namespace Zapper.LoyaltyPoints.Infrastructure.Configuration;

public interface IMessagingConfiguration
{
    MessagingProvider Provider { get; }
}

public class SqsMessagingConfiguration : IMessagingConfiguration
{
    public MessagingProvider Provider => MessagingProvider.Sqs;
    public SqsSettings SqsSettings { get; }

    public SqsMessagingConfiguration(SqsSettings sqsSettings)
    {
        SqsSettings = sqsSettings;
    }
}

public class DevEmulatorMessagingConfiguration : IMessagingConfiguration
{
    public MessagingProvider Provider => MessagingProvider.DevEmulator;
    public DevEmulatorSettings DevEmulatorSettings { get; }

    public DevEmulatorMessagingConfiguration(DevEmulatorSettings devEmulatorSettings)
    {
        DevEmulatorSettings = devEmulatorSettings;
    }
}

public class InMemoryMessagingConfiguration : IMessagingConfiguration
{
    public MessagingProvider Provider => MessagingProvider.InMemory;
}
