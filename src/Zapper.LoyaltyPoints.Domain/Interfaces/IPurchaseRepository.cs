namespace Zapper.LoyaltyPoints.Domain.Interfaces;

public interface IPurchaseRepository
{
    Task<Purchase?> GetByTransactionIdAsync(string transactionId, CancellationToken ct = default);
    Task<bool> ExistsAsync(string transactionId, CancellationToken ct = default);
    Task AddAsync(Purchase purchase, CancellationToken ct = default);
    Task UpdateAsync(Purchase purchase, CancellationToken ct = default);
    Task<Purchase?> FindRecentAsync(string customerId, string merchantId, decimal amount, string currency, TimeSpan window, CancellationToken ct = default);
}
