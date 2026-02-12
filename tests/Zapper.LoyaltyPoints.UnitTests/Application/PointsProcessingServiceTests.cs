using FluentAssertions;
using Moq;
using Zapper.LoyaltyPoints.Application.Responses;
using Zapper.LoyaltyPoints.Application.Services;
using Zapper.LoyaltyPoints.Domain.Entities;
using Zapper.LoyaltyPoints.Domain.Interfaces;

namespace Zapper.LoyaltyPoints.UnitTests.Application;

public class PointsProcessingServiceTests
{
    private readonly Mock<IPurchaseRepository> _purchaseRepo = new();
    private readonly Mock<IPointsLedgerRepository> _ledgerRepo = new();
    private readonly Mock<ICustomerBalanceRepository> _balanceRepo = new();
    private readonly Mock<IPointsCalculationService> _strategy = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly PointsProcessingService _service;

    public PointsProcessingServiceTests()
    {
        _service = new PointsProcessingService(
            _purchaseRepo.Object,
            _ledgerRepo.Object,
            _balanceRepo.Object,
            _strategy.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task ProcessPurchasePoints_NewTransaction_ShouldAwardPoints()
    {
        var purchaseEvent = CreateTestPurchaseEvent();
        _ledgerRepo.Setup(r => r.ExistsByTransactionIdAsync(purchaseEvent.TransactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _strategy.Setup(s => s.CalculatePoints(purchaseEvent.Amount, purchaseEvent.Currency)).Returns(24);
        _balanceRepo.Setup(r => r.GetAsync(purchaseEvent.CustomerId, purchaseEvent.MerchantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerBalance?)null);
        var purchase = Purchase.Create(purchaseEvent.TransactionId, purchaseEvent.MerchantId, purchaseEvent.CustomerId, purchaseEvent.Amount, purchaseEvent.Currency, purchaseEvent.Timestamp, purchaseEvent.PaymentMethod);
        _purchaseRepo.Setup(r => r.GetByTransactionIdAsync(purchaseEvent.TransactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchase);

        await _service.ProcessPurchasePointsAsync(purchaseEvent);

        _ledgerRepo.Verify(r => r.AddAsync(It.IsAny<PointsLedgerEntry>(), It.IsAny<CancellationToken>()), Times.Once);
        _balanceRepo.Verify(r => r.AddAsync(It.IsAny<CustomerBalance>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPurchasePoints_DuplicateTransaction_ShouldSkip()
    {
        var purchaseEvent = CreateTestPurchaseEvent();
        _ledgerRepo.Setup(r => r.ExistsByTransactionIdAsync(purchaseEvent.TransactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _service.ProcessPurchasePointsAsync(purchaseEvent);

        _ledgerRepo.Verify(r => r.AddAsync(It.IsAny<PointsLedgerEntry>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPurchasePoints_ExistingBalance_ShouldUpdateNotCreate()
    {
        var purchaseEvent = CreateTestPurchaseEvent();
        var existingBalance = CustomerBalance.Create(purchaseEvent.CustomerId, purchaseEvent.MerchantId);
        existingBalance.AddPoints(10);

        _ledgerRepo.Setup(r => r.ExistsByTransactionIdAsync(purchaseEvent.TransactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _strategy.Setup(s => s.CalculatePoints(purchaseEvent.Amount, purchaseEvent.Currency)).Returns(24);
        _balanceRepo.Setup(r => r.GetAsync(purchaseEvent.CustomerId, purchaseEvent.MerchantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBalance);
        var purchase = Purchase.Create(purchaseEvent.TransactionId, purchaseEvent.MerchantId, purchaseEvent.CustomerId, purchaseEvent.Amount, purchaseEvent.Currency, purchaseEvent.Timestamp, purchaseEvent.PaymentMethod);
        _purchaseRepo.Setup(r => r.GetByTransactionIdAsync(purchaseEvent.TransactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchase);

        await _service.ProcessPurchasePointsAsync(purchaseEvent);

        _balanceRepo.Verify(r => r.UpdateAsync(It.IsAny<CustomerBalance>(), It.IsAny<CancellationToken>()), Times.Once);
        _balanceRepo.Verify(r => r.AddAsync(It.IsAny<CustomerBalance>(), It.IsAny<CancellationToken>()), Times.Never);
        existingBalance.TotalPoints.Should().Be(34); // 10 + 24
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static PurchaseEventResponse CreateTestPurchaseEvent() => new()
    {
        TransactionId = "m-123-abc",
        MerchantId = "zcoffee-001",
        CustomerId = "cust-789",
        Amount = 245.70m,
        Currency = "ZAR",
        Timestamp = DateTime.UtcNow,
        PaymentMethod = "card"
    };
}
