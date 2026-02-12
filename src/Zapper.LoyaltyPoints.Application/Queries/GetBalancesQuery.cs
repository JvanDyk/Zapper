namespace Zapper.LoyaltyPoints.Application.Queries;

public sealed record GetBalancesQuery(
    string CustomerId) : IRequest<CustomerBalanceSummaryResponse>;
