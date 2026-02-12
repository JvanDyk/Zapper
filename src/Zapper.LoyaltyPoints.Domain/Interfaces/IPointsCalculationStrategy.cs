namespace Zapper.LoyaltyPoints.Domain.Interfaces;

public interface IPointsCalculationStrategy
{
    int CalculateRounding(decimal amount, string currency);
}
