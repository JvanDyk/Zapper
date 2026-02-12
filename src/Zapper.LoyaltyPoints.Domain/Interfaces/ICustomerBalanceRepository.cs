namespace Zapper.LoyaltyPoints.Domain.Interfaces;

public interface ICustomerBalanceRepository
{
    Task<CustomerBalance?> GetAsync(string customerId, string merchantId, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerBalance>> GetAllByCustomerAsync(string customerId, CancellationToken ct = default);
    Task AddAsync(CustomerBalance balance, CancellationToken ct = default);
    Task UpdateAsync(CustomerBalance balance, CancellationToken ct = default);
}
