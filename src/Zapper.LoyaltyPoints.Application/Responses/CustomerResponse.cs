using System.Text.Json.Serialization;

namespace Zapper.LoyaltyPoints.Application.Responses;

public sealed record CustomerResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("customer_code")]
    public string CustomerCode { get; init; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }
}
