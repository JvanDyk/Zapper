using System.Text.Json.Serialization;

namespace Zapper.LoyaltyPoints.Application.Responses;

public sealed record CustomerBalanceResponse
{
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = default!;

    [JsonPropertyName("merchant_id")]
    public string MerchantId { get; init; } = default!;

    [JsonPropertyName("total_points")]
    public int TotalPoints { get; init; }
}

public sealed record CustomerBalanceSummaryResponse
{
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = default!;

    [JsonPropertyName("balances")]
    public IReadOnlyList<CustomerBalanceResponse> Balances { get; init; } = [];
}
