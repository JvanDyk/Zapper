namespace Zapper.LoyaltyPoints.Application.Queries;

public sealed record GetMerchantBalanceQuery(string CustomerId, string MerchantId) : IRequest<CustomerBalanceResponse>;
