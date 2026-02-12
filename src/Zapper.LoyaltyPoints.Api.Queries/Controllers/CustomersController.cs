namespace Zapper.LoyaltyPoints.Api.Queries.Controllers;

using Zapper.LoyaltyPoints.Infrastructure.Models;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<CustomerResponse>> GetAll(CancellationToken ct)
    {
        return await mediator.Send(new GetAllCustomersQuery(), ct);
    }

    [HttpGet("{customerCode}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    public async Task<CustomerResponse> GetByCode(string customerCode, CancellationToken ct)
    {
        return await mediator.Send(new GetCustomerByCodeQuery(customerCode), ct);
    }
}
