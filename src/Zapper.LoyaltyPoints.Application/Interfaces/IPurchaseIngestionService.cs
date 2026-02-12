namespace Zapper.LoyaltyPoints.Application.Interfaces;

public interface IPurchaseIngestionService
{
    Task<PurchaseResponse> IngestPurchaseAsync(PurchaseEventResponse purchaseEvent, CancellationToken ct = default);
}
