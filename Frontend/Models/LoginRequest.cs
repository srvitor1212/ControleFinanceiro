using System.Text.Json.Serialization;

namespace Frontend.Models;

public sealed class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; init; } = string.Empty;
}
