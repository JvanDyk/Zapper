using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zapper.LoyaltyPoints.Infrastructure.Persistence;

namespace Zapper.LoyaltyPoints.IntegrationTests;

public abstract class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private readonly string _dbName = "TestDb_" + Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("AppSettings:Messaging:Provider", "InMemory");
        builder.UseSetting("Messaging:Provider", "InMemory");
        builder.UseSetting("Environment", "Testing");
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var toRemove = services.Where(d =>
            {
                var stName = d.ServiceType.FullName ?? "";
                var itName = d.ImplementationType?.FullName ?? "";
                return stName.Contains("EntityFrameworkCore") ||
                       stName.Contains("Npgsql") ||
                       itName.Contains("Npgsql") ||
                       d.ServiceType == typeof(LoyaltyDbContext);
            }).ToList();

            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            services.AddDbContext<LoyaltyDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }
}

public sealed class CommandApiFactory : CustomWebApplicationFactory<Zapper.LoyaltyPoints.Api.Commands.Program>;
