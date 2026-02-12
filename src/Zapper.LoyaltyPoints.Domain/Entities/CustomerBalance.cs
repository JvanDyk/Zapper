namespace Zapper.LoyaltyPoints.Domain.Entities;

public class CustomerBalance : BaseEntity
{
    public string CustomerId { get; private set; } = default!;
    public string MerchantId { get; private set; } = default!;
    public int TotalPoints { get; private set; }

    public int Version { get; private set; }

    private CustomerBalance() { } // EF Core

    public static CustomerBalance Create(string customerId, string merchantId)
    {
        return new CustomerBalance
        {
            CustomerId = customerId,
            MerchantId = merchantId,
            TotalPoints = 0,
            Version = 1
        };
    }

    public void AddPoints(int points)
    {
        if (points < 0)
            throw new ArgumentException("Cannot add negative points.", nameof(points));

        TotalPoints += points;
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }
}
