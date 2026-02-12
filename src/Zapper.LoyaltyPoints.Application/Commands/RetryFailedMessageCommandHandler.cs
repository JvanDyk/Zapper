using Zapper.LoyaltyPoints.Domain.Entities.Queue;
using Zapper.LoyaltyPoints.Domain.Exceptions;

namespace Zapper.LoyaltyPoints.Application.Commands;

public sealed class RetryFailedMessageCommandHandler(
    IQueueRepository? queueRepository = null) : IRequestHandler<RetryFailedMessageCommand, QueueRetryResultResponse>
{
    public async Task<QueueRetryResultResponse> Handle(RetryFailedMessageCommand request, CancellationToken cancellationToken)
    {
        if (queueRepository is null)
            throw new ConflictException("Queue retry is only available when using the DevEmulator provider.");

        var failedMessages = await queueRepository.GetRetryableFailedMessagesAsync(cancellationToken);
        var target = failedMessages.FirstOrDefault(f => f.Id == request.FailedMessageId);

        if (target is null)
            throw new EntityNotFoundException("FailedMessage", request.FailedMessageId.ToString());
        var newMessage = new QueueMessage(target.MessageType, target.Payload, maxRetries: 3);
        await queueRepository.AddAsync(newMessage, cancellationToken);
        target.MarkRetried();
        await queueRepository.UpdateFailedMessageAsync(target, cancellationToken);

        var response = new QueueRetryResultResponse
        {
            Message = "Message re-queued successfully.",
            NewMessageId = newMessage.Id,
            OriginalFailedId = request.FailedMessageId
        };

        return response;
    }
}
