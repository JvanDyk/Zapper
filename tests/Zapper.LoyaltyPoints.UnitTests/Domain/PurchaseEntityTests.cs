using FluentAssertions;
using Zapper.LoyaltyPoints.Domain.Entities;

namespace Zapper.LoyaltyPoints.UnitTests.Domain;

public class PurchaseEntityTests
{
    [Fact]
    public void Create_ValidInput_ShouldCreatePurchase()
    {
        var purchase = Purchase.Create("tx-1", "merchant-1", "cust-1", 100m, "ZAR", DateTime.UtcNow, "card");

        purchase.TransactionId.Should().Be("tx-1");
        purchase.MerchantId.Should().Be("merchant-1");
        purchase.CustomerId.Should().Be("cust-1");
        purchase.Amount.Should().Be(100m);
        purchase.Currency.Should().Be("ZAR");
        purchase.PointsAwarded.Should().BeFalse();
    }

    [Fact]
    public void Create_EmptyTransactionId_ShouldThrow()
    {
        var act = () => Purchase.Create("", "merchant-1", "cust-1", 100m, "ZAR", DateTime.UtcNow, "card");
        act.Should().Throw<ArgumentException>().WithParameterName("transactionId");
    }

    [Fact]
    public void Create_ZeroAmount_ShouldThrow()
    {
        var act = () => Purchase.Create("tx-1", "merchant-1", "cust-1", 0m, "ZAR", DateTime.UtcNow, "card");
        act.Should().Throw<ArgumentException>().WithParameterName("amount");
    }

    [Fact]
    public void MarkPointsAwarded_ShouldSetFlag()
    {
        var purchase = Purchase.Create("tx-1", "merchant-1", "cust-1", 100m, "ZAR", DateTime.UtcNow, "card");
        purchase.MarkPointsAwarded();

        purchase.PointsAwarded.Should().BeTrue();
        purchase.UpdatedAt.Should().NotBeNull();
    }
}
