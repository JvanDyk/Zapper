namespace Zapper.LoyaltyPoints.Application.Queries;

public sealed record GetMerchantByCodeQuery(string MerchantCode) : IRequest<MerchantResponse>;
