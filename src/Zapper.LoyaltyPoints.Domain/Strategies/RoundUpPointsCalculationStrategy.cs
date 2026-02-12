namespace Zapper.LoyaltyPoints.Domain.Strategies;

public sealed class RoundUpPointsCalculationStrategy : IPointsCalculationStrategy
{
    private const decimal PointsPerUnit = 10m;

    public int CalculateRounding(decimal amount, string currency)
    {
        if (amount <= 0) return 0;
        var rawPoints = amount / PointsPerUnit;
        return (int)RoundUp(rawPoints);
    }

    private static decimal RoundUp(decimal value) =>
        Math.Round(value, MidpointRounding.AwayFromZero);
}
