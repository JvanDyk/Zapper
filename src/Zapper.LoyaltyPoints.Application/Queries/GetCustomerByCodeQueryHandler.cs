namespace Zapper.LoyaltyPoints.Application.Queries;

using Zapper.LoyaltyPoints.Domain.Exceptions;

public sealed class GetCustomerByCodeQueryHandler(
    ICustomerRepository customerRepository) : IRequestHandler<GetCustomerByCodeQuery, CustomerResponse>
{
    public async Task<CustomerResponse> Handle(GetCustomerByCodeQuery request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByCodeAsync(request.CustomerCode, cancellationToken);
        if (customer is null)
            throw new EntityNotFoundException("Customer", request.CustomerCode);

        var response = new CustomerResponse
        {
            Id = customer.Id,
            CustomerCode = customer.CustomerCode,
            Name = customer.Name,
            Email = customer.Email,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        };

        return response;
    }
}
