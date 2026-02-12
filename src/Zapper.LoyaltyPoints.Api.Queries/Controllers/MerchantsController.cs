namespace Zapper.LoyaltyPoints.Api.Queries.Controllers;

using Zapper.LoyaltyPoints.Infrastructure.Models;

[ApiController]
[Route("api/merchants")]
public sealed class MerchantsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MerchantResponse>), StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<MerchantResponse>> GetAll(CancellationToken ct)
    {
        return await mediator.Send(new GetAllMerchantsQuery(), ct);
    }

    [HttpGet("{merchantCode}")]
    [ProducesResponseType(typeof(MerchantResponse), StatusCodes.Status200OK)]
    public async Task<MerchantResponse> GetByCode(string merchantCode, CancellationToken ct)
    {
        return await mediator.Send(new GetMerchantByCodeQuery(merchantCode), ct);
    }
}
