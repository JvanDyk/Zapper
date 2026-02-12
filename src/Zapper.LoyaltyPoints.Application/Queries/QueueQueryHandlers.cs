namespace Zapper.LoyaltyPoints.Application.Queries;

public sealed class GetQueueStatusQueryHandler(
    IQueueRepository queueRepository) : IRequestHandler<GetQueueStatusQuery, QueueStatusResponse>
{
    public async Task<QueueStatusResponse> Handle(GetQueueStatusQuery request, CancellationToken cancellationToken)
    {

        var pending = await queueRepository.GetPendingCountAsync(cancellationToken);
        var processing = await queueRepository.GetProcessingCountAsync(cancellationToken);
        var deadLetter = await queueRepository.GetDeadLetterCountAsync(cancellationToken);
        var completed = await queueRepository.GetCompletedCountAsync(cancellationToken);

        var response = new QueueStatusResponse
        {
            Pending = pending,
            Processing = processing,
            DeadLetter = deadLetter,
            Completed = completed,
            Total = pending + processing + deadLetter + completed
        };

        return response;
    }
}

public sealed class GetDeadLettersQueryHandler(
    IQueueRepository queueRepository) : IRequestHandler<GetDeadLettersQuery, IReadOnlyList<FailedMessageResponse>>
{
    public async Task<IReadOnlyList<FailedMessageResponse>> Handle(GetDeadLettersQuery request, CancellationToken cancellationToken)
    {

        var failed = await queueRepository.GetRetryableFailedMessagesAsync(cancellationToken);

        var response = failed.Select(f => new FailedMessageResponse
        {
            Id = f.Id,
            OriginalMessageId = f.OriginalMessageId,
            MessageType = f.MessageType,
            Payload = f.Payload,
            LastError = f.LastErrorMessage,
            TotalAttempts = f.TotalAttempts,
            CanRetry = f.CanRetry,
            FailedAt = f.FailedAt,
            RetriedAt = f.RetriedAt
        }).ToList();

        return response;
    }
}

public sealed class GetQueueHistoryQueryHandler(
    IQueueRepository queueRepository) : IRequestHandler<GetQueueHistoryQuery, IReadOnlyList<QueueHistoryEntryResponse>>
{
    public async Task<IReadOnlyList<QueueHistoryEntryResponse>> Handle(GetQueueHistoryQuery request, CancellationToken cancellationToken)
    {

        var history = await queueRepository.GetHistoryByMessageIdAsync(request.MessageId, cancellationToken);

        var response = history.Select(h => new QueueHistoryEntryResponse
        {
            Id = h.Id,
            QueueMessageId = h.QueueMessageId,
            MessageType = h.MessageType,
            Status = h.Status.ToString(),
            ProcessedAt = h.ProcessedAt,
            ElapsedMs = h.ElapsedMs,
            ErrorMessage = h.ErrorMessage,
            AttemptNumber = h.AttemptNumber
        }).ToList();

        return response;
    }
}
