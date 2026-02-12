using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Zapper.LoyaltyPoints.Infrastructure.Configuration;

namespace Zapper.LoyaltyPoints.Infrastructure.Messaging;

public sealed class SqsMessageConsumer(
    IAmazonSQS sqsClient,
    AppSettings appSettings,
    ILogger<SqsMessageConsumer> logger)
{
    private readonly SqsSettings _settings = appSettings.Aws.Sqs;
    private string? _queueUrl;

    public async Task<IReadOnlyList<Message>> ReceiveBatchAsync(CancellationToken ct)
    {
        var queueUrl = await ResolveQueueUrlAsync(ct);

        var request = new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = _settings.MaxNumberOfMessages,
            WaitTimeSeconds = _settings.WaitTimeSeconds,
            VisibilityTimeout = _settings.VisibilityTimeoutSeconds
        };

        var response = await sqsClient.ReceiveMessageAsync(request, ct);
        return response.Messages;
    }

    public async Task DeleteAsync(string receiptHandle, CancellationToken ct)
    {
        var queueUrl = await ResolveQueueUrlAsync(ct);

        await sqsClient.DeleteMessageAsync(new DeleteMessageRequest
        {
            QueueUrl = queueUrl,
            ReceiptHandle = receiptHandle
        }, ct);
    }

    private async Task<string> ResolveQueueUrlAsync(CancellationToken ct)
    {
        if (_queueUrl is not null) return _queueUrl;

        var response = await sqsClient.GetQueueUrlAsync(
            new GetQueueUrlRequest { QueueName = _settings.QueueName }, ct);

        _queueUrl = response.QueueUrl;

        logger.LogInformation("Resolved SQS queue URL: {QueueUrl}", _queueUrl);
        return _queueUrl;
    }
}
