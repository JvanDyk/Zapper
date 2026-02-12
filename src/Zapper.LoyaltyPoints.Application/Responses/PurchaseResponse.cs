using System.Text.Json.Serialization;

namespace Zapper.LoyaltyPoints.Application.Responses;

public sealed record PurchaseResponse
{
    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; init; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; init; } = default!;

    [JsonPropertyName("message")]
    public string Message { get; init; } = default!;
}
