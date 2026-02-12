using FluentAssertions;
using Zapper.LoyaltyPoints.Domain.Strategies;

namespace Zapper.LoyaltyPoints.UnitTests.Domain;

public class RoundUpPointsCalculationStrategyTests
{
    private readonly RoundUpPointsCalculationStrategy _strategy = new();

    [Theory]
    [InlineData(245.70, "ZAR", 25)]   // 245.70 / 10 = 24.57 → round up = 25
    [InlineData(100.00, "ZAR", 10)]   // exact
    [InlineData(9.99, "ZAR", 1)]      // 0.999 → round up = 1
    [InlineData(10.00, "ZAR", 1)]     // exactly 1 point
    [InlineData(15.00, "ZAR", 2)]     // 1.5 → round up = 2
    [InlineData(0, "ZAR", 0)]         // zero amount
    [InlineData(999.99, "ZAR", 100)]  // 99.999 → round up = 100
    [InlineData(1000.00, "ZAR", 100)] // round number
    public void CalculateRounding_ShouldReturnCorrectPoints(decimal amount, string currency, int expected)
    {
        var result = _strategy.CalculateRounding(amount, currency);
        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateRounding_NegativeAmount_ShouldReturnZero()
    {
        var result = _strategy.CalculateRounding(-50m, "ZAR");
        result.Should().Be(0);
    }
}
