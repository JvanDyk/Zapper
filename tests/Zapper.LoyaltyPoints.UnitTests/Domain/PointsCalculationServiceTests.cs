using FluentAssertions;
using Microsoft.Extensions.Options;
using Zapper.LoyaltyPoints.Infrastructure.Configuration;
using Zapper.LoyaltyPoints.Infrastructure.Models.Enums;
using Zapper.LoyaltyPoints.Infrastructure.Services;

namespace Zapper.LoyaltyPoints.UnitTests.Domain;

public class PointsCalculationServiceTests
{
    [Theory]
    [InlineData(245.70, "ZAR", 24)]   // 245.70 / 10 = 24.57 → floor = 24
    [InlineData(100.00, "ZAR", 10)]   // exact
    [InlineData(15.00, "ZAR", 1)]     // 1.5 → floor = 1
    public void CalculatePoints_WithStandardStrategy_ShouldUseFloor(decimal amount, string currency, int expected)
    {
        var settings = new AppSettings
        {
            PointsCalculation = new PointsCalculationSettings
            {
                Strategy = PointsCalculationStrategy.Standard
            }
        };
        var options = Options.Create(settings);
        var service = new PointsCalculationService(options);

        var result = service.CalculatePoints(amount, currency);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(245.70, "ZAR", 25)]   // 245.70 / 10 = 24.57 → round up = 25
    [InlineData(100.00, "ZAR", 10)]   // exact
    [InlineData(15.00, "ZAR", 2)]     // 1.5 → round up = 2
    public void CalculatePoints_WithRoundUpStrategy_ShouldUseRoundUp(decimal amount, string currency, int expected)
    {
        var settings = new AppSettings
        {
            PointsCalculation = new PointsCalculationSettings
            {
                Strategy = PointsCalculationStrategy.RoundUp
            }
        };
        var options = Options.Create(settings);
        var service = new PointsCalculationService(options);

        var result = service.CalculatePoints(amount, currency);

        result.Should().Be(expected);
    }

    [Fact]
    public void CalculatePoints_ZeroAmount_ShouldReturnZero_ForBothStrategies()
    {
        var standardSettings = new AppSettings
        {
            PointsCalculation = new PointsCalculationSettings { Strategy = PointsCalculationStrategy.Standard }
        };
        var roundUpSettings = new AppSettings
        {
            PointsCalculation = new PointsCalculationSettings { Strategy = PointsCalculationStrategy.RoundUp }
        };

        var standardService = new PointsCalculationService(Options.Create(standardSettings));
        var roundUpService = new PointsCalculationService(Options.Create(roundUpSettings));

        standardService.CalculatePoints(0, "ZAR").Should().Be(0);
        roundUpService.CalculatePoints(0, "ZAR").Should().Be(0);
    }

    [Fact]
    public void CalculatePoints_NegativeAmount_ShouldReturnZero_ForBothStrategies()
    {
        var standardSettings = new AppSettings
        {
            PointsCalculation = new PointsCalculationSettings { Strategy = PointsCalculationStrategy.Standard }
        };
        var roundUpSettings = new AppSettings
        {
            PointsCalculation = new PointsCalculationSettings { Strategy = PointsCalculationStrategy.RoundUp }
        };

        var standardService = new PointsCalculationService(Options.Create(standardSettings));
        var roundUpService = new PointsCalculationService(Options.Create(roundUpSettings));

        standardService.CalculatePoints(-50m, "ZAR").Should().Be(0);
        roundUpService.CalculatePoints(-50m, "ZAR").Should().Be(0);
    }
}
