namespace Zapper.LoyaltyPoints.Domain.Strategies;

public sealed class StandardPointsCalculationStrategy : IPointsCalculationStrategy
{
    private const decimal PointsPerUnit = 10m;

    public int CalculateRounding(decimal amount, string currency)
    {
        if (amount <= 0) return 0;
        return (int)Math.Floor(amount / PointsPerUnit);
    }
}
