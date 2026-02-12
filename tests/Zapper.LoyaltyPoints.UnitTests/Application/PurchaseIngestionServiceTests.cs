using FluentAssertions;
using Moq;
using Zapper.LoyaltyPoints.Application.Responses;
using Zapper.LoyaltyPoints.Application.Services;
using Zapper.LoyaltyPoints.Domain.Entities;
using Zapper.LoyaltyPoints.Domain.Exceptions;
using Zapper.LoyaltyPoints.Domain.Interfaces;

namespace Zapper.LoyaltyPoints.UnitTests.Application;

public class PurchaseIngestionServiceTests
{
    private readonly Mock<IPurchaseRepository> _purchaseRepo = new();
    private readonly Mock<IMessagePublisher> _messagePublisher = new();
    private readonly Mock<IPointsCalculationService> _calculationStrategy = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly PurchaseIngestionService _service;

    public PurchaseIngestionServiceTests()
    {
        _calculationStrategy.Setup(s => s.CalculatePoints(It.IsAny<decimal>(), It.IsAny<string>())).Returns(100);
        _service = new PurchaseIngestionService(
            _purchaseRepo.Object,
            _messagePublisher.Object,
            _calculationStrategy.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task IngestPurchase_NewTransaction_ShouldPersistAndPublish()
    {
        var purchaseEvent = CreateTestPurchaseEvent();
        _purchaseRepo.Setup(r => r.GetByTransactionIdAsync(purchaseEvent.TransactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Purchase?)null);
        _purchaseRepo.Setup(r => r.FindRecentAsync(purchaseEvent.CustomerId, purchaseEvent.MerchantId, purchaseEvent.Amount, purchaseEvent.Currency, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Purchase?)null);

        var result = await _service.IngestPurchaseAsync(purchaseEvent);

        result.Status.Should().Be("accepted");
        result.TransactionId.Should().Be(purchaseEvent.TransactionId);
        _purchaseRepo.Verify(r => r.AddAsync(It.IsAny<Purchase>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _messagePublisher.Verify(p => p.PublishAsync(purchaseEvent, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IngestPurchase_DuplicateTransaction_ShouldThrowDuplicateException()
    {
        var purchaseEvent = CreateTestPurchaseEvent();
        var existingPurchase = Purchase.Create(purchaseEvent.TransactionId, purchaseEvent.MerchantId, purchaseEvent.CustomerId, purchaseEvent.Amount, purchaseEvent.Currency, purchaseEvent.Timestamp, purchaseEvent.PaymentMethod);
        _purchaseRepo.Setup(r => r.GetByTransactionIdAsync(purchaseEvent.TransactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPurchase);

        var act = () => _service.IngestPurchaseAsync(purchaseEvent);

        await act.Should().ThrowAsync<DuplicateTransactionException>();
        _purchaseRepo.Verify(r => r.AddAsync(It.IsAny<Purchase>(), It.IsAny<CancellationToken>()), Times.Never);
        _messagePublisher.Verify(p => p.PublishAsync(It.IsAny<PurchaseEventResponse>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task IngestPurchase_NearDuplicate_ShouldThrowDuplicateException()
    {
        var purchaseEvent = CreateTestPurchaseEvent();
        _purchaseRepo.Setup(r => r.GetByTransactionIdAsync(purchaseEvent.TransactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Purchase?)null);

        var recentPurchase = Purchase.Create("other-tx-id", purchaseEvent.MerchantId, purchaseEvent.CustomerId, purchaseEvent.Amount, purchaseEvent.Currency, purchaseEvent.Timestamp, purchaseEvent.PaymentMethod);
        _purchaseRepo.Setup(r => r.FindRecentAsync(purchaseEvent.CustomerId, purchaseEvent.MerchantId, purchaseEvent.Amount, purchaseEvent.Currency, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentPurchase);

        var act = () => _service.IngestPurchaseAsync(purchaseEvent);

        await act.Should().ThrowAsync<DuplicateTransactionException>();
        _purchaseRepo.Verify(r => r.AddAsync(It.IsAny<Purchase>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task IngestPurchase_OldTimestamp_ShouldThrowInvalidOperationException()
    {
        var purchaseEvent = CreateTestPurchaseEvent(timestamp: DateTime.UtcNow.AddMinutes(-10)); // 10 minutes ago (older than 5-minute window)
        
        _purchaseRepo.Setup(r => r.GetByTransactionIdAsync(purchaseEvent.TransactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Purchase?)null);
        _purchaseRepo.Setup(r => r.FindRecentAsync(purchaseEvent.CustomerId, purchaseEvent.MerchantId, purchaseEvent.Amount, purchaseEvent.Currency, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Purchase?)null);

        var act = () => _service.IngestPurchaseAsync(purchaseEvent);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*too old*")
            .WithMessage("*within 5 minutes*");
        _purchaseRepo.Verify(r => r.AddAsync(It.IsAny<Purchase>(), It.IsAny<CancellationToken>()), Times.Never);
        _messagePublisher.Verify(p => p.PublishAsync(It.IsAny<PurchaseEventResponse>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static PurchaseEventResponse CreateTestPurchaseEvent(DateTime? timestamp = null) => new()
    {
        TransactionId = "m-123-abc",
        MerchantId = "zcoffee-001",
        CustomerId = "cust-789",
        Amount = 245.70m,
        Currency = "ZAR",
        Timestamp = timestamp ?? DateTime.UtcNow,
        PaymentMethod = "card"
    };
}
