namespace Zapper.LoyaltyPoints.Api.Commands.Controllers;

[ApiController]
[Route("api/queue")]
public sealed class QueueController(IMediator mediator) : ControllerBase
{
    [HttpPost("retry/{failedMessageId:guid}")]
    [ProducesResponseType(typeof(QueueRetryResultResponse), StatusCodes.Status200OK)]
    public async Task<QueueRetryResultResponse> RetryFailedMessage(Guid failedMessageId, CancellationToken ct)
    {
        return await mediator.Send(new RetryFailedMessageCommand(failedMessageId), ct);
    }
}
