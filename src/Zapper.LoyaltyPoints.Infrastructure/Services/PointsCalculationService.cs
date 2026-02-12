using Microsoft.Extensions.Options;
using Zapper.LoyaltyPoints.Domain.Interfaces;
using Zapper.LoyaltyPoints.Domain.Strategies;
using Zapper.LoyaltyPoints.Infrastructure.Configuration;
using Zapper.LoyaltyPoints.Infrastructure.Models.Enums;

namespace Zapper.LoyaltyPoints.Infrastructure.Services;

public sealed class PointsCalculationService : IPointsCalculationService

{
    private readonly IPointsCalculationStrategy _strategy;

    public PointsCalculationService(IOptions<AppSettings> appSettings)
    {
        var settings = appSettings.Value;
        _strategy = settings.PointsCalculation.Strategy switch
        {
            PointsCalculationStrategy.RoundUp => new RoundUpPointsCalculationStrategy(),
            _ => new StandardPointsCalculationStrategy()
        };
    }

    public int CalculatePoints(decimal amount, string currency)
        => _strategy.CalculateRounding(amount, currency);
}
