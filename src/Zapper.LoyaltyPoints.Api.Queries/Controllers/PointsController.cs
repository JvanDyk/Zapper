namespace Zapper.LoyaltyPoints.Api.Queries.Controllers;

using Zapper.LoyaltyPoints.Infrastructure.Models;

[ApiController]
[Route("api/customers/{customerId}/[controller]")]
public sealed class PointsController(IMediator mediator) : ControllerBase
{
    [HttpGet("balance")]
    [ProducesResponseType(typeof(CustomerBalanceSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<CustomerBalanceSummaryResponse> GetBalances(string customerId, CancellationToken ct)
    {
        return await mediator.Send(new GetBalancesQuery(customerId), ct);
    }

    [HttpGet("balance/{merchantId}")]
    [ProducesResponseType(typeof(CustomerBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<CustomerBalanceResponse> GetMerchantBalance(string customerId, string merchantId, CancellationToken ct)
    {
        return await mediator.Send(new GetMerchantBalanceQuery(customerId, merchantId), ct);
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(PointsHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<PointsHistoryResponse> GetHistory(
        string customerId,
        [FromQuery] string? merchantId = null,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0,
        CancellationToken ct = default)
    {
        return await mediator.Send(new GetPointsHistoryQuery(customerId, merchantId, limit, offset), ct);
    }
}
