namespace Zapper.LoyaltyPoints.Application.Queries;

public sealed class GetAllCustomersQueryHandler(
    ICustomerRepository customerRepository) : IRequestHandler<GetAllCustomersQuery, IReadOnlyList<CustomerResponse>>
{
    public async Task<IReadOnlyList<CustomerResponse>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await customerRepository.GetAllAsync(cancellationToken);

        var response = customers.Select(c => new CustomerResponse
        {
            Id = c.Id,
            CustomerCode = c.CustomerCode,
            Name = c.Name,
            Email = c.Email,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        }).ToList();

        return response;
    }
}
