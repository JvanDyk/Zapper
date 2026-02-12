using System.Text.Json.Serialization;

namespace Zapper.LoyaltyPoints.Application.Responses;

public sealed record PointsLedgerEntryResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; init; } = default!;

    [JsonPropertyName("merchant_id")]
    public string MerchantId { get; init; } = default!;

    [JsonPropertyName("points")]
    public int Points { get; init; }

    [JsonPropertyName("purchase_amount")]
    public decimal PurchaseAmount { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; } = default!;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }
}

public sealed record PointsHistoryResponse
{
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = default!;

    [JsonPropertyName("entries")]
    public IReadOnlyList<PointsLedgerEntryResponse> Entries { get; init; } = [];
}
