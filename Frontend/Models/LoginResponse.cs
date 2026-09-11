using System.Text.Json.Serialization;

namespace Frontend.Models;

public sealed class LoginResponse
{
    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; init; }

    [JsonPropertyName("tokenType")]
    public string? TokenType { get; init; }

    [JsonPropertyName("expiresAt")]
    public string? ExpiresAt { get; init; }
}
