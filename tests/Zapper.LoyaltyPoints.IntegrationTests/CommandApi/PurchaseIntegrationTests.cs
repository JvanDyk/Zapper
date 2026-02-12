using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Zapper.LoyaltyPoints.Application.Responses;
using Zapper.LoyaltyPoints.Infrastructure.Models;

namespace Zapper.LoyaltyPoints.IntegrationTests;

public class PurchaseIntegrationTests : IClassFixture<CommandApiFactory>
{
    private readonly HttpClient _client;

    public PurchaseIntegrationTests(CommandApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static int _amountSeed = 0;

    private static PurchaseEventResponse CreatePurchaseEvent(
        string? transactionId = null,
        string? customerId = null,
        string? merchantId = null,
        decimal? amount = null,
        string? currency = null,
        DateTime? timestamp = null) => new()
    {
        TransactionId = transactionId ?? $"test-{Guid.NewGuid():N}",
        MerchantId = merchantId ?? "zcoffee-001",
        CustomerId = customerId ?? "cust-test-1",
        Amount = amount ?? (100m + Interlocked.Increment(ref _amountSeed)),
        Currency = currency ?? "ZAR",
        Timestamp = timestamp ?? DateTime.UtcNow,
        PaymentMethod = "card"
    };

    [Fact]
    public async Task IngestPurchase_ValidEvent_Returns202Accepted()
    {
        var purchase = CreatePurchaseEvent();

        var response = await _client.PostAsJsonAsync("/api/purchases", purchase);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var body = await response.Content.ReadFromJsonAsync<PurchaseResponse>();
        body.Should().NotBeNull();
        body!.TransactionId.Should().Be(purchase.TransactionId);
        body.Status.Should().Be("accepted");
    }

    [Fact]
    public async Task IngestPurchase_DuplicateTransaction_Returns409Conflict()
    {
        var purchase = CreatePurchaseEvent();

        // First request should succeed
        var response1 = await _client.PostAsJsonAsync("/api/purchases", purchase);
        response1.StatusCode.Should().Be(HttpStatusCode.Accepted);

        // Second request with same transaction ID should conflict
        var response2 = await _client.PostAsJsonAsync("/api/purchases", purchase);
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await response2.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Code.Should().Be("DUPLICATE_TRANSACTION");
    }

    [Fact]
    public async Task IngestPurchase_NegativeAmount_Returns400()
    {
        var purchase = CreatePurchaseEvent(amount: -10m);

        var response = await _client.PostAsJsonAsync("/api/purchases", purchase);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task IngestPurchase_MissingTransactionId_Returns400()
    {
        var purchase = CreatePurchaseEvent(transactionId: "");

        var response = await _client.PostAsJsonAsync("/api/purchases", purchase);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task IngestPurchase_OldTimestamp_Returns400()
    {
        var purchase = CreatePurchaseEvent(timestamp: DateTime.UtcNow.AddMinutes(-10)); // 10 minutes ago (older than 5-minute window)

        var response = await _client.PostAsJsonAsync("/api/purchases", purchase);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Code.Should().Be("VALIDATION_ERROR");
        body.Message.Should().Contain("too old");
        body.Message.Should().Contain("within 5 minutes");
    }

    [Fact]
    public async Task IngestPurchase_NearDuplicate_Returns409Conflict()
    {
        var purchase = CreatePurchaseEvent();

        // First request should succeed
        var response1 = await _client.PostAsJsonAsync("/api/purchases", purchase);
        response1.StatusCode.Should().Be(HttpStatusCode.Accepted);

        // Second request with same customer/merchant/amount but different transaction ID within 5 minutes should conflict
        var nearDuplicate = CreatePurchaseEvent(
            transactionId: $"near-duplicate-{Guid.NewGuid():N}",
            customerId: purchase.CustomerId,
            merchantId: purchase.MerchantId,
            amount: purchase.Amount,
            currency: purchase.Currency,
            timestamp: DateTime.UtcNow.AddMinutes(-2) // Within 5-minute window
        );

        var response2 = await _client.PostAsJsonAsync("/api/purchases", nearDuplicate);
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await response2.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Code.Should().Be("DUPLICATE_TRANSACTION");
        body.Message.Should().Contain("similar purchase");
        body.Message.Should().Contain("within the last 5 minutes");
    }
}
