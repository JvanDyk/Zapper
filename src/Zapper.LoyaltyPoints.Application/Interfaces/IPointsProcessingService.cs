namespace Zapper.LoyaltyPoints.Application.Interfaces;

public interface IPointsProcessingService
{
    Task ProcessPurchasePointsAsync(PurchaseEventResponse purchaseEvent, CancellationToken ct = default);
}
