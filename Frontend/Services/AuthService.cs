using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Frontend.Models;

namespace Frontend.Services;

public sealed class AuthService : IAuthService
{
    private readonly HttpClient httpClient = new();
    private readonly Uri loginUri;

    public AuthService(Uri authBaseUri)
    {
        loginUri = new Uri(authBaseUri, "api/auth/login");
    }

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                loginUri,
                new LoginRequest { Email = email, Password = password },
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
                return HasUsableLoginContract(loginResponse)
                    ? LoginResult.Success(loginResponse!)
                    : LoginResult.Failure();
            }

            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return LoginResult.Rejected(FlattenErrors(content));
            }

            return LoginResult.Failure();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return LoginResult.Failure();
        }
    }

    private static bool HasUsableLoginContract(LoginResponse? response) =>
        !string.IsNullOrWhiteSpace(response?.AccessToken)
        && !string.IsNullOrWhiteSpace(response.ExpiresAt);

    private static IReadOnlyList<string> FlattenErrors(string content)
    {
        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty("errors", out var errors))
            {
                return Array.Empty<string>();
            }

            return GetMessages(errors).ToArray();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    private static IEnumerable<string> GetMessages(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                if (!string.IsNullOrWhiteSpace(element.GetString()))
                {
                    yield return element.GetString()!;
                }

                yield break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    foreach (var message in GetMessages(item))
                    {
                        yield return message;
                    }
                }

                yield break;
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    foreach (var message in GetMessages(property.Value))
                    {
                        yield return message;
                    }
                }

                yield break;
            default:
                yield break;
        }
    }
}
