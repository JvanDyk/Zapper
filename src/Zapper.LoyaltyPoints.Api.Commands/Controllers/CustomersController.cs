namespace Zapper.LoyaltyPoints.Api.Commands.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateCustomerCommand requestCommand, CancellationToken ct)
    {
        var response = await mediator.Send(requestCommand, ct);
        return Created(string.Empty, response);
    }
}
