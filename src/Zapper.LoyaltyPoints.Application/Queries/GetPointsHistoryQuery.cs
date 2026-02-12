namespace Zapper.LoyaltyPoints.Application.Queries;

public sealed record GetPointsHistoryQuery(
    string CustomerId,
    string? MerchantId = null,
    int Limit = 20,
    int Offset = 0) : IRequest<PointsHistoryResponse>;
