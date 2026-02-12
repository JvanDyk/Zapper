using FluentAssertions;
using Zapper.LoyaltyPoints.Domain.Entities;

namespace Zapper.LoyaltyPoints.UnitTests.Domain;

public class CustomerBalanceTests
{
    [Fact]
    public void Create_ShouldInitializeWithZeroPoints()
    {
        var balance = CustomerBalance.Create("cust-1", "merchant-1");

        balance.CustomerId.Should().Be("cust-1");
        balance.MerchantId.Should().Be("merchant-1");
        balance.TotalPoints.Should().Be(0);
        balance.Version.Should().Be(1);
    }

    [Fact]
    public void AddPoints_ShouldIncrementTotalAndVersion()
    {
        var balance = CustomerBalance.Create("cust-1", "merchant-1");
        balance.AddPoints(24);

        balance.TotalPoints.Should().Be(24);
        balance.Version.Should().Be(2);
    }

    [Fact]
    public void AddPoints_MultipleTimes_ShouldAccumulate()
    {
        var balance = CustomerBalance.Create("cust-1", "merchant-1");
        balance.AddPoints(10);
        balance.AddPoints(15);
        balance.AddPoints(5);

        balance.TotalPoints.Should().Be(30);
        balance.Version.Should().Be(4);
    }

    [Fact]
    public void AddPoints_NegativeAmount_ShouldThrow()
    {
        var balance = CustomerBalance.Create("cust-1", "merchant-1");
        var act = () => balance.AddPoints(-5);
        act.Should().Throw<ArgumentException>();
    }
}
