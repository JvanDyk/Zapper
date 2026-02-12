namespace Zapper.LoyaltyPoints.Application.Queries;

public sealed record GetQueueStatusQuery : IRequest<QueueStatusResponse>;
public sealed record GetDeadLettersQuery : IRequest<IReadOnlyList<FailedMessageResponse>>;
public sealed record GetQueueHistoryQuery(Guid MessageId) : IRequest<IReadOnlyList<QueueHistoryEntryResponse>>;
