using Zapper.LoyaltyPoints.Domain.Entities;
using Zapper.LoyaltyPoints.Infrastructure.Persistence;

namespace Zapper.LoyaltyPoints.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(LoyaltyDbContext context)
    {
        if (context.Purchases.Any() || context.CustomerBalances.Any())
        {
            return; // Already seeded
        }

        var now = DateTime.UtcNow;
        var merchants = new[]
        {
            Merchant.Create("zcoffee-001", "Z-Coffee Roasters", "info@zcoffee.co.za"),
            Merchant.Create("zbookstore-002", "Z-Bookstore", "hello@zbookstore.co.za"),
            Merchant.Create("zgrocery-003", "Z-Grocery", "support@zgrocery.co.za"),
            Merchant.Create("MERCH001", "Test Merchant 001", "test@merchant001.com")
        };
        context.Merchants.AddRange(merchants);
        var customers = new[]
        {
            Customer.Create("cust-demo-1", "Thabo Mokoena", "thabo@example.com"),
            Customer.Create("cust-demo-2", "Naledi Dlamini", "naledi@example.com"),
            Customer.Create("cust-demo-3", "Sipho Nkosi", "sipho@example.com"),
            Customer.Create("CUST001", "Test Customer 001", "test@customer001.com")
        };
        context.Customers.AddRange(customers);
        var purchases = new[]
        {
            Purchase.Create("seed-001", "zcoffee-001", "cust-demo-1", 245.70m, "ZAR", now.AddHours(-2), "card"),
            Purchase.Create("seed-002", "zcoffee-001", "cust-demo-1", 150.00m, "ZAR", now.AddHours(-1), "card"),
            Purchase.Create("seed-003", "zbookstore-002", "cust-demo-2", 89.99m, "ZAR", now.AddMinutes(-30), "card"),
            Purchase.Create("seed-004", "zgrocery-003", "cust-demo-3", 456.50m, "ZAR", now.AddMinutes(-15), "card")
        };
        foreach (var p in purchases.Take(3))
        {
            p.MarkPointsAwarded();
        }

        context.Purchases.AddRange(purchases);
        var ledgerEntries = new[]
        {
            PointsLedgerEntry.Create("cust-demo-1", "zcoffee-001", "seed-001", 24, 245.70m, "1 point per 10 ZAR"),
            PointsLedgerEntry.Create("cust-demo-1", "zcoffee-001", "seed-002", 15, 150.00m, "1 point per 10 ZAR"),
            PointsLedgerEntry.Create("cust-demo-2", "zbookstore-002", "seed-003", 8, 89.99m, "1 point per 10 ZAR")
        };
        context.PointsLedgerEntries.AddRange(ledgerEntries);
        var balances = new[]
        {
            CustomerBalance.Create("cust-demo-1", "zcoffee-001"),
            CustomerBalance.Create("cust-demo-2", "zbookstore-002")
        };
        balances[0].AddPoints(39); // 24 + 15
        balances[1].AddPoints(8);
        context.CustomerBalances.AddRange(balances);

        await context.SaveChangesAsync();
    }
}
