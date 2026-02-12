namespace Zapper.LoyaltyPoints.Application.Commands;

public sealed record ProcessPointsCommand(PurchaseEventResponse PurchaseEvent) : IRequest<Unit>;
