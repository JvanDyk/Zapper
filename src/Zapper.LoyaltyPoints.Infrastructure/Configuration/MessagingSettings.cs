using Zapper.LoyaltyPoints.Infrastructure.Models.Enums;

namespace Zapper.LoyaltyPoints.Infrastructure.Configuration;

public sealed class MessagingSettings
{
    public const string SectionName = "Messaging";

    public MessagingProvider Provider { get; init; }
}
