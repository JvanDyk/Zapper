using Microsoft.EntityFrameworkCore;
using Zapper.LoyaltyPoints.Domain.Entities;
using Zapper.LoyaltyPoints.Domain.Interfaces;
using Zapper.LoyaltyPoints.Infrastructure.Persistence;

namespace Zapper.LoyaltyPoints.Infrastructure.Repositories;

public sealed class PurchaseRepository(LoyaltyDbContext context) : IPurchaseRepository
{

    public async Task<Purchase?> GetByTransactionIdAsync(string transactionId, CancellationToken ct = default)
    {
        return await context.Purchases
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, ct);
    }

    public async Task<bool> ExistsAsync(string transactionId, CancellationToken ct = default)
    {
        return await context.Purchases
            .AnyAsync(p => p.TransactionId == transactionId, ct);
    }

    public async Task AddAsync(Purchase purchase, CancellationToken ct = default)
    {
        await context.Purchases.AddAsync(purchase, ct);
    }

    public Task UpdateAsync(Purchase purchase, CancellationToken ct = default)
    {
        context.Purchases.Update(purchase);
        return Task.CompletedTask;
    }

    public async Task<Purchase?> FindRecentAsync(
        string customerId, string merchantId, decimal amount, string currency,
        TimeSpan window, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow - window;
        return await context.Purchases
            .Where(p => p.CustomerId == customerId
                     && p.MerchantId == merchantId
                     && p.Amount == amount
                     && p.Currency == currency
                     && p.CreatedAt >= cutoff)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }
}
