namespace Zapper.LoyaltyPoints.Application.Commands;

public sealed record IngestPurchaseCommand(PurchaseEventResponse PurchaseEvent) : IRequest<PurchaseResponse>;
