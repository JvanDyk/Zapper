namespace Zapper.LoyaltyPoints.Application.Queries;

public sealed record GetCustomerByCodeQuery(string CustomerCode) : IRequest<CustomerResponse>;
