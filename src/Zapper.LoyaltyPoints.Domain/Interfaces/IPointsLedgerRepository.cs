namespace Zapper.LoyaltyPoints.Domain.Interfaces;

public interface IPointsLedgerRepository
{
    Task AddAsync(PointsLedgerEntry entry, CancellationToken ct = default);
    Task<bool> ExistsByTransactionIdAsync(string transactionId, CancellationToken ct = default);
    Task<IReadOnlyList<PointsLedgerEntry>> GetByCustomerAndMerchantAsync(
        string customerId, string merchantId, int limit = 20, int offset = 0, CancellationToken ct = default);
    Task<IReadOnlyList<PointsLedgerEntry>> GetByCustomerAsync(
        string customerId, int limit = 20, int offset = 0, CancellationToken ct = default);
}
