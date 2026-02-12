using Microsoft.EntityFrameworkCore;
using Zapper.LoyaltyPoints.Domain.Entities;
using Zapper.LoyaltyPoints.Domain.Interfaces;
using Zapper.LoyaltyPoints.Infrastructure.Persistence;

namespace Zapper.LoyaltyPoints.Infrastructure.Repositories;

public sealed class CustomerBalanceRepository(LoyaltyDbContext context) : ICustomerBalanceRepository
{

    public async Task<CustomerBalance?> GetAsync(string customerId, string merchantId, CancellationToken ct = default)
    {
        return await context.CustomerBalances
            .FirstOrDefaultAsync(b => b.CustomerId == customerId && b.MerchantId == merchantId, ct);
    }

    public async Task<IReadOnlyList<CustomerBalance>> GetAllByCustomerAsync(string customerId, CancellationToken ct = default)
    {
        return await context.CustomerBalances
            .Where(b => b.CustomerId == customerId)
            .OrderBy(b => b.MerchantId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(CustomerBalance balance, CancellationToken ct = default)
    {
        await context.CustomerBalances.AddAsync(balance, ct);
    }

    public Task UpdateAsync(CustomerBalance balance, CancellationToken ct = default)
    {
        context.CustomerBalances.Update(balance);
        return Task.CompletedTask;
    }
}
