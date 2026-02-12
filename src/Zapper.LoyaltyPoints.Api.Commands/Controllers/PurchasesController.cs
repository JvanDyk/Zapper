namespace Zapper.LoyaltyPoints.Api.Commands.Controllers;

using Zapper.LoyaltyPoints.Infrastructure.Models;

[ApiController]
[Route("api/[controller]")]
public sealed class PurchasesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(PurchaseResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> IngestPurchase(
        [FromBody] PurchaseEventResponse purchaseEvent,
        CancellationToken ct)
    {
        var response = await mediator.Send(new IngestPurchaseCommand(purchaseEvent), ct);
        return Accepted(response);
    }
}
