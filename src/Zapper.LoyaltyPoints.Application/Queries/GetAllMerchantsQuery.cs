namespace Zapper.LoyaltyPoints.Application.Queries;

public sealed record GetAllMerchantsQuery : IRequest<IReadOnlyList<MerchantResponse>>;
