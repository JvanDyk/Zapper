using Zapper.LoyaltyPoints.Application.Commands;
using Zapper.LoyaltyPoints.Domain.Entities;
using Zapper.LoyaltyPoints.Domain.Exceptions;
using Zapper.LoyaltyPoints.Domain.Interfaces;

namespace Zapper.LoyaltyPoints.Application.Services;

public sealed class PurchaseIngestionService(
    IPurchaseRepository purchaseRepository,
    IMessagePublisher messagePublisher,
    IPointsCalculationService calculationStrategy,
    IUnitOfWork unitOfWork) : IRequestHandler<IngestPurchaseCommand, PurchaseResponse>
{
    private static readonly TimeSpan NearDuplicateWindow = TimeSpan.FromMinutes(5);
    
    public async Task<PurchaseResponse> IngestPurchaseAsync(PurchaseEventResponse purchaseEvent, CancellationToken ct = default)
    {
        // Check if transaction already exists by ID
        var existing = await purchaseRepository.GetByTransactionIdAsync(purchaseEvent.TransactionId, ct);
        if (existing is not null)
            throw new DuplicateTransactionException(purchaseEvent.TransactionId);

        // Validate purchase timestamp is recent (within 5 minutes)
        var timeSincePurchase = DateTime.UtcNow - purchaseEvent.Timestamp;
        if (timeSincePurchase > NearDuplicateWindow)
        {
            throw new InvalidOperationException(
                $"Purchase timestamp '{purchaseEvent.Timestamp:yyyy-MM-dd HH:mm:ss UTC}' is too old. " +
                $"Purchase events must be within {NearDuplicateWindow.TotalMinutes} minutes of current time. " +
                $"Time difference: {timeSincePurchase.TotalMinutes:F1} minutes.");
        }

        // Check for near-duplicate purchases (same customer, merchant, amount within 5 minutes)
        var recentPurchase = await purchaseRepository.FindRecentAsync(
            purchaseEvent.CustomerId, purchaseEvent.MerchantId, purchaseEvent.Amount, purchaseEvent.Currency, NearDuplicateWindow, ct);

        if (recentPurchase is not null)
        {
            throw new DuplicateTransactionException(
                purchaseEvent.TransactionId,
                $"A similar purchase for customer '{purchaseEvent.CustomerId}' at merchant '{purchaseEvent.MerchantId}' " +
                $"with amount {purchaseEvent.Amount} {purchaseEvent.Currency} was submitted within the last {NearDuplicateWindow.TotalMinutes} minutes " +
                $"(existing transaction: {recentPurchase.TransactionId}, submitted at {recentPurchase.TransactionTimestamp:yyyy-MM-dd HH:mm:ss UTC}). " +
                $"Current purchase timestamp: {purchaseEvent.Timestamp:yyyy-MM-dd HH:mm:ss UTC}. " +
                "If this is intentional, please use a unique transaction_id.");
        }
        
        // Create and persist purchase entity
        var purchase = Purchase.Create(
            purchaseEvent.TransactionId,
            purchaseEvent.MerchantId,
            purchaseEvent.CustomerId,
            purchaseEvent.Amount,
            purchaseEvent.Currency,
            purchaseEvent.Timestamp,
            purchaseEvent.PaymentMethod);

        await purchaseRepository.AddAsync(purchase, ct);
        await unitOfWork.SaveChangesAsync(ct);
        await messagePublisher.PublishAsync(purchaseEvent, ct);

        // Calculate points and return response
        var pointsEarned = calculationStrategy.CalculatePoints(purchaseEvent.Amount, purchaseEvent.Currency);

        return new PurchaseResponse
        {
            TransactionId = purchaseEvent.TransactionId,
            Status = "accepted",
            Message = $"Purchase accepted. Rewarded {pointsEarned} points."
        };
    }

    public Task<PurchaseResponse> Handle(IngestPurchaseCommand request, CancellationToken cancellationToken)
        => IngestPurchaseAsync(request.PurchaseEvent, cancellationToken);
}
