namespace Zapper.LoyaltyPoints.Application.Commands;

public sealed record RetryFailedMessageCommand(Guid FailedMessageId) : IRequest<QueueRetryResultResponse>;
