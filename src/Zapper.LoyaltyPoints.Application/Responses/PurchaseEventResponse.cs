using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Zapper.LoyaltyPoints.Application.Responses;

public sealed record PurchaseEventResponse
{
    [Required]
    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; init; } = default!;

    [Required]
    [JsonPropertyName("merchant_id")]
    public string MerchantId { get; init; } = default!;

    [Required]
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = default!;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive.")]
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [Required]
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = default!;

    [Required]
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }

    [Required]
    [JsonPropertyName("payment_method")]
    public string PaymentMethod { get; init; } = default!;
}
