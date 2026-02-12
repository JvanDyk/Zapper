using Microsoft.EntityFrameworkCore;
using Zapper.LoyaltyPoints.Domain.Entities;
using Zapper.LoyaltyPoints.Domain.Interfaces;
using Zapper.LoyaltyPoints.Infrastructure.Persistence;

namespace Zapper.LoyaltyPoints.Infrastructure.Repositories;

public sealed class MerchantRepository(LoyaltyDbContext context) : IMerchantRepository
{

    public async Task<Merchant?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Merchants.FindAsync(new object[] { id }, ct);

    public async Task<Merchant?> GetByCodeAsync(string merchantCode, CancellationToken ct = default)
        => await context.Merchants.FirstOrDefaultAsync(m => m.MerchantCode == merchantCode, ct);

    public async Task<IReadOnlyList<Merchant>> GetAllAsync(CancellationToken ct = default)
        => await context.Merchants.OrderBy(m => m.Name).ToListAsync(ct);

    public async Task AddAsync(Merchant merchant, CancellationToken ct = default)
        => await context.Merchants.AddAsync(merchant, ct);

    public Task UpdateAsync(Merchant merchant, CancellationToken ct = default)
    {
        context.Merchants.Update(merchant);
        return Task.CompletedTask;
    }
}
