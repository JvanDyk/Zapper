using System.Text.Json.Serialization;

namespace Zapper.LoyaltyPoints.Infrastructure.Models;

public record ErrorResponse(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message);
