namespace Zapper.LoyaltyPoints.Domain.Interfaces;

public interface IMerchantRepository
{
    Task<Merchant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Merchant?> GetByCodeAsync(string merchantCode, CancellationToken ct = default);
    Task<IReadOnlyList<Merchant>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Merchant merchant, CancellationToken ct = default);
    Task UpdateAsync(Merchant merchant, CancellationToken ct = default);
}
