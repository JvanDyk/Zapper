using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Zapper.LoyaltyPoints.Domain.Interfaces;
using Zapper.LoyaltyPoints.Infrastructure.Configuration;

namespace Zapper.LoyaltyPoints.Infrastructure.Messaging;

public sealed class SqsMessagePublisher(
    IAmazonSQS sqsClient,
    AppSettings appSettings,
    ILogger<SqsMessagePublisher> logger) : IMessagePublisher
{
    private readonly SqsSettings _settings = appSettings.Aws.Sqs;
    private string? _queueUrl;

    public async Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        var queueUrl = await ResolveQueueUrlAsync(ct);
        var messageBody = JsonSerializer.Serialize(message);

        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = messageBody
        };

        var response = await sqsClient.SendMessageAsync(request, ct);

        logger.LogInformation("Published {MessageType} to SQS. MessageId: {MessageId}",
            typeof(T).Name, response.MessageId);
    }

    private async Task<string> ResolveQueueUrlAsync(CancellationToken ct)
    {
        if (_queueUrl is not null) return _queueUrl;

        var response = await sqsClient.GetQueueUrlAsync(
            new GetQueueUrlRequest { QueueName = _settings.QueueName }, ct);

        _queueUrl = response.QueueUrl;
        return _queueUrl;
    }
}
