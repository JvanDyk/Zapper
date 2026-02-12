using Zapper.LoyaltyPoints.Infrastructure.Models.Enums;

namespace Zapper.LoyaltyPoints.Infrastructure.Configuration;

public sealed class PointsCalculationSettings
{
    public PointsCalculationStrategy Strategy { get; init; }
}
