using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Zapper.LoyaltyPoints.Domain.Interfaces;

namespace Zapper.LoyaltyPoints.Infrastructure.Messaging;

public sealed class InMemoryMessagePublisher : IMessagePublisher
{
    private readonly Channel<string> _channel;
    private readonly ILogger<InMemoryMessagePublisher> _logger;

    public InMemoryMessagePublisher(Channel<string> channel, ILogger<InMemoryMessagePublisher> logger)
    {
        _channel = channel;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        var json = JsonSerializer.Serialize(message);
        await _channel.Writer.WriteAsync(json, ct);
        _logger.LogInformation("[InMemory] Published message: {MessageType}", typeof(T).Name);
    }
}
public sealed class InMemoryMessageConsumer
{
    private readonly Channel<string> _channel;

    public InMemoryMessageConsumer(Channel<string> channel)
    {
        _channel = channel;
    }

    public ChannelReader<string> Reader => _channel.Reader;
}
