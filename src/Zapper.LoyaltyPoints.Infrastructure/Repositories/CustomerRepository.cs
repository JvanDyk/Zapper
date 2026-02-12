using Microsoft.EntityFrameworkCore;
using Zapper.LoyaltyPoints.Domain.Entities;
using Zapper.LoyaltyPoints.Domain.Interfaces;
using Zapper.LoyaltyPoints.Infrastructure.Persistence;

namespace Zapper.LoyaltyPoints.Infrastructure.Repositories;

public sealed class CustomerRepository(LoyaltyDbContext context) : ICustomerRepository
{

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Customers.FindAsync(new object[] { id }, ct);

    public async Task<Customer?> GetByCodeAsync(string customerCode, CancellationToken ct = default)
        => await context.Customers.FirstOrDefaultAsync(c => c.CustomerCode == customerCode, ct);

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default)
        => await context.Customers.OrderBy(c => c.Name).ToListAsync(ct);

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
        => await context.Customers.AddAsync(customer, ct);

    public Task UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        context.Customers.Update(customer);
        return Task.CompletedTask;
    }
}
