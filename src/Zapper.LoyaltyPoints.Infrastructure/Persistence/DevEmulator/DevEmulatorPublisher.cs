using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zapper.LoyaltyPoints.Domain.Entities.Queue;
using Zapper.LoyaltyPoints.Domain.Interfaces;

namespace Zapper.LoyaltyPoints.Infrastructure.Persistence.DevEmulator;

public sealed class DevEmulatorPublisher : IMessagePublisher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DevEmulatorPublisher> _logger;

    public DevEmulatorPublisher(IServiceScopeFactory scopeFactory, ILogger<DevEmulatorPublisher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        var payload = JsonSerializer.Serialize(message);
        var queueMessage = new QueueMessage(
            messageType: typeof(T).Name,
            payload: payload,
            maxRetries: 3);

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LoyaltyDbContext>();

        await context.QueueMessages.AddAsync(queueMessage, ct);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "[DevEmulator] Published message {MessageId} type={MessageType}",
            queueMessage.Id, queueMessage.MessageType);
    }
}
