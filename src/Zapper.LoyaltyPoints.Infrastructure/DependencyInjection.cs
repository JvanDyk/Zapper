using Microsoft.Extensions.Options;
using Zapper.LoyaltyPoints.Infrastructure.Configuration;
using System.Threading.Channels;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zapper.LoyaltyPoints.Domain.Interfaces;
using Zapper.LoyaltyPoints.Infrastructure.Services;
using Zapper.LoyaltyPoints.Infrastructure.Messaging;
using Zapper.LoyaltyPoints.Infrastructure.Persistence;
using Zapper.LoyaltyPoints.Infrastructure.Persistence.DevEmulator;
using Zapper.LoyaltyPoints.Infrastructure.Repositories;
using Zapper.LoyaltyPoints.Infrastructure.Models.Enums;

namespace Zapper.LoyaltyPoints.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettings>>().Value);
        
        // Get AppSettings once and reuse it
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>() ?? throw new InvalidOperationException("AppSettings configuration is required");
        
        services.AddDbContext<LoyaltyDbContext>((sp, options) =>
        {
            options.UseNpgsql(appSettings.ConnectionStrings.LoyaltyDb);
        });
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IPointsLedgerRepository, PointsLedgerRepository>();
        services.AddScoped<ICustomerBalanceRepository, CustomerBalanceRepository>();
        services.AddScoped<IMerchantRepository, MerchantRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<LoyaltyDbContext>());

        services.AddScoped<IPointsCalculationService, PointsCalculationService>();
        
        services.AddSingleton<IMessagingConfiguration>(sp =>
        {
            return appSettings.Messaging.Provider switch
            {
                MessagingProvider.Sqs => new SqsMessagingConfiguration(appSettings.Aws.Sqs),
                MessagingProvider.DevEmulator => new DevEmulatorMessagingConfiguration(appSettings.DevEmulator),
                _ => new InMemoryMessagingConfiguration()
            };
        });

        if (configuration["Messaging:Provider"] == "InMemory" && configuration["Environment"] == "Testing")
        {
            services.AddSingleton<Channel<string>>(Channel.CreateUnbounded<string>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            }));
        }
        else if (configuration["Messaging:Provider"] == "Sqs")
        {
            services.AddSingleton<IAmazonSQS>(sp =>
            {
                var sqsSettings = appSettings.Aws.Sqs;
                var awsConfig = new AmazonSQSConfig
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(sqsSettings.Region)
                };
                return new AmazonSQSClient(awsConfig);
            });
        }

        services.AddSingleton<IMessagePublisher>(sp =>
        {
            var config = sp.GetRequiredService<IMessagingConfiguration>();
            return config.Provider switch
            {
                MessagingProvider.Sqs => new SqsMessagePublisher(sp.GetRequiredService<IAmazonSQS>(), appSettings, sp.GetRequiredService<ILogger<SqsMessagePublisher>>()),
                MessagingProvider.DevEmulator => new Persistence.DevEmulator.DevEmulatorPublisher(sp.GetRequiredService<IServiceScopeFactory>(), sp.GetRequiredService<ILogger<DevEmulatorPublisher>>()),
                _ => new InMemoryMessagePublisher(
                    sp.GetService<Channel<string>>() ?? throw new InvalidOperationException("Channel<string> not registered. Ensure Messaging:Provider=InMemory and Environment=Testing are configured."),
                    sp.GetRequiredService<ILogger<InMemoryMessagePublisher>>())
            };
        });

        if (configuration["AppSettings:Messaging:Provider"] == "DevEmulator")
        {
            services.Configure<DevEmulatorSettings>(configuration.GetSection($"AppSettings:{DevEmulatorSettings.SectionName}"));
            services.AddScoped<IQueueRepository>(sp =>
                new QueueRepository(sp.GetRequiredService<LoyaltyDbContext>()));
            services.AddSingleton<DevEmulatorConsumer>();
        }

        return services;
    }
}
