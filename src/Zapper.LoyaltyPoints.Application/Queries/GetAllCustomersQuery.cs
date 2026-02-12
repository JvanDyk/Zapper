namespace Zapper.LoyaltyPoints.Application.Queries;

public sealed record GetAllCustomersQuery : IRequest<IReadOnlyList<CustomerResponse>>;
