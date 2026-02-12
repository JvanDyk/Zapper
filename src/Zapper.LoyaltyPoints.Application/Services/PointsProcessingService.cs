using Zapper.LoyaltyPoints.Application.Commands;
using Zapper.LoyaltyPoints.Domain.Entities;
using Zapper.LoyaltyPoints.Domain.Interfaces;

namespace Zapper.LoyaltyPoints.Application.Services;

public sealed class PointsProcessingService(
    IPurchaseRepository purchaseRepository,
    IPointsLedgerRepository ledgerRepository,
    ICustomerBalanceRepository balanceRepository,
    IPointsCalculationService calculationStrategy,
    IUnitOfWork unitOfWork) : IRequestHandler<ProcessPointsCommand, Unit>
{
    public async Task ProcessPurchasePointsAsync(PurchaseEventResponse purchaseEvent, CancellationToken ct = default)
    {
        // Check if points were already awarded for this transaction
        var alreadyAwarded = await ledgerRepository.ExistsByTransactionIdAsync(purchaseEvent.TransactionId, ct);
        if (alreadyAwarded)
            return;
        
        // Calculate points and create ledger entry
        var points = calculationStrategy.CalculatePoints(purchaseEvent.Amount, purchaseEvent.Currency);
        var ledgerEntry = PointsLedgerEntry.Create(
            purchaseEvent.CustomerId,
            purchaseEvent.MerchantId,
            purchaseEvent.TransactionId,
            points,
            purchaseEvent.Amount,
            $"Points earned: {points} pts for {purchaseEvent.Currency} {purchaseEvent.Amount:F2} purchase");

        await ledgerRepository.AddAsync(ledgerEntry, ct);
        
        // Update or create customer balance for this merchant
        var balance = await balanceRepository.GetAsync(purchaseEvent.CustomerId, purchaseEvent.MerchantId, ct);
        if (balance is null)
        {
            balance = CustomerBalance.Create(purchaseEvent.CustomerId, purchaseEvent.MerchantId);
            balance.AddPoints(points);
            await balanceRepository.AddAsync(balance, ct);
        }
        else
        {
            balance.AddPoints(points);
            await balanceRepository.UpdateAsync(balance, ct);
        }
        
        // Mark purchase as having points awarded
        var purchase = await purchaseRepository.GetByTransactionIdAsync(purchaseEvent.TransactionId, ct);
        if (purchase is { PointsAwarded: false })
        {
            purchase.MarkPointsAwarded();
            await purchaseRepository.UpdateAsync(purchase, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<Unit> Handle(ProcessPointsCommand request, CancellationToken cancellationToken)
    {
        await ProcessPurchasePointsAsync(request.PurchaseEvent, cancellationToken);
        return Unit.Value;
    }
}
