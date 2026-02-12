namespace Zapper.LoyaltyPoints.Api.Queries.Controllers;

using Zapper.LoyaltyPoints.Infrastructure.Models;

[ApiController]
[Route("api/queue")]
public sealed class QueueController(IMediator mediator) : ControllerBase
{
    [HttpGet("status")]
    [ProducesResponseType(typeof(QueueStatusResponse), StatusCodes.Status200OK)]
    public async Task<QueueStatusResponse> GetStatus(CancellationToken ct)
    {
        return await mediator.Send(new GetQueueStatusQuery(), ct);
    }

    [HttpGet("dead-letters")]
    [ProducesResponseType(typeof(IReadOnlyList<FailedMessageResponse>), StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<FailedMessageResponse>> GetDeadLetters(CancellationToken ct)
    {
        return await mediator.Send(new GetDeadLettersQuery(), ct);
    }

    [HttpGet("history/{messageId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<QueueHistoryEntryResponse>), StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<QueueHistoryEntryResponse>> GetHistory(Guid messageId, CancellationToken ct)
    {
        return await mediator.Send(new GetQueueHistoryQuery(messageId), ct);
    }
}
