using Microsoft.EntityFrameworkCore;
using Zapper.LoyaltyPoints.Domain.Entities;
using Zapper.LoyaltyPoints.Domain.Interfaces;
using Zapper.LoyaltyPoints.Infrastructure.Persistence;

namespace Zapper.LoyaltyPoints.Infrastructure.Repositories;

public sealed class PointsLedgerRepository(LoyaltyDbContext context) : IPointsLedgerRepository
{

    public async Task AddAsync(PointsLedgerEntry entry, CancellationToken ct = default)
    {
        await context.PointsLedgerEntries.AddAsync(entry, ct);
    }

    public async Task<bool> ExistsByTransactionIdAsync(string transactionId, CancellationToken ct = default)
    {
        return await context.PointsLedgerEntries
            .AnyAsync(e => e.TransactionId == transactionId, ct);
    }

    public async Task<IReadOnlyList<PointsLedgerEntry>> GetByCustomerAndMerchantAsync(
        string customerId, string merchantId, int limit = 20, int offset = 0, CancellationToken ct = default)
    {
        return await context.PointsLedgerEntries
            .Where(e => e.CustomerId == customerId && e.MerchantId == merchantId)
            .OrderByDescending(e => e.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PointsLedgerEntry>> GetByCustomerAsync(
        string customerId, int limit = 20, int offset = 0, CancellationToken ct = default)
    {
        return await context.PointsLedgerEntries
            .Where(e => e.CustomerId == customerId)
            .OrderByDescending(e => e.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);
    }
}
