namespace Zapper.LoyaltyPoints.Domain.Entities;

public class PointsLedgerEntry : BaseEntity
{
    public string CustomerId { get; private set; } = default!;
    public string MerchantId { get; private set; } = default!;
    public string TransactionId { get; private set; } = default!;
    public int Points { get; private set; }

    public decimal PurchaseAmount { get; private set; }

    public string Description { get; private set; } = default!;

    private PointsLedgerEntry() { }

    public static PointsLedgerEntry Create(
        string customerId,
        string merchantId,
        string transactionId,
        int points,
        decimal purchaseAmount,
        string description)
    {
        if (points < 0)
            throw new ArgumentException("Points cannot be negative.", nameof(points));

        return new PointsLedgerEntry
        {
            CustomerId = customerId,
            MerchantId = merchantId,
            TransactionId = transactionId,
            Points = points,
            PurchaseAmount = purchaseAmount,
            Description = description
        };
    }
}
