namespace Zapper.LoyaltyPoints.Domain.Entities;

public class Purchase : BaseEntity
{
    public string TransactionId { get; private set; }
    public string MerchantId { get; private set; }
    public string CustomerId { get; private set; }
    public decimal Amount { get; private set; }

    public string Currency { get; private set; }
    public DateTime TransactionTimestamp { get; private set; }

    public string PaymentMethod { get; private set; }
    public bool PointsAwarded { get; private set; }

    private Purchase() { } // EF Core

    public static Purchase Create(
        string transactionId,
        string merchantId,
        string customerId,
        decimal amount,
        string currency,
        DateTime transactionTimestamp,
        string paymentMethod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(merchantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(amount, 0);

        return new Purchase
        {
            TransactionId = transactionId,
            MerchantId = merchantId,
            CustomerId = customerId,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            TransactionTimestamp = transactionTimestamp,
            PaymentMethod = paymentMethod,
            PointsAwarded = false
        };
    }

    public void MarkPointsAwarded()
    {
        PointsAwarded = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
