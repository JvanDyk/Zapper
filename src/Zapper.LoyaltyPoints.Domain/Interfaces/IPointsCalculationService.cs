namespace Zapper.LoyaltyPoints.Domain.Interfaces;

public interface IPointsCalculationService
{
    int CalculatePoints(decimal amount, string currency);
}
