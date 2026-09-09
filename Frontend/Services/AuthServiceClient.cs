using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Frontend.Services;

public sealed class AuthServiceClient(HttpClient httpClient)
{
    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/auth/login",
            new LoginRequest(email, password));

        if (response.IsSuccessStatusCode)
        {
            return LoginResult.Success();
        }

        return LoginResult.Failure(await GetErrorsAsync(response));
    }

    private static async Task<IReadOnlyList<string>> GetErrorsAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                using var document = JsonDocument.Parse(content);

                if (document.RootElement.TryGetProperty("errors", out var errors))
                {
                    var messages = GetErrorMessages(errors);
                    if (messages.Count > 0)
                    {
                        return messages;
                    }
                }

                if (document.RootElement.TryGetProperty("detail", out var detail)
                    && detail.ValueKind == JsonValueKind.String
                    && !string.IsNullOrWhiteSpace(detail.GetString()))
                {
                    return [detail.GetString()!];
                }
            }
            catch (JsonException)
            {
                // A resposta não segue um formato de erro JSON conhecido.
            }
        }

        return [$"Não foi possível realizar o login ({(int)response.StatusCode} {GetStatusDescription(response.StatusCode)})."];
    }

    private static IReadOnlyList<string> GetErrorMessages(JsonElement errors)
    {
        var messages = new List<string>();

        if (errors.ValueKind == JsonValueKind.Array)
        {
            AddStringValues(errors, messages);
        }
        else if (errors.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in errors.EnumerateObject())
            {
                AddStringValues(property.Value, messages);
            }
        }

        return messages;
    }

    private static void AddStringValues(JsonElement element, ICollection<string> messages)
    {
        if (element.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(element.GetString()))
        {
            messages.Add(element.GetString()!);
            return;
        }

        if (element.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var value in element.EnumerateArray())
        {
            if (value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString()))
            {
                messages.Add(value.GetString()!);
            }
        }
    }

    private static string GetStatusDescription(HttpStatusCode statusCode) =>
        statusCode switch
        {
            HttpStatusCode.BadRequest => "requisição inválida",
            HttpStatusCode.Unauthorized => "credenciais inválidas",
            _ => "erro inesperado"
        };

    private sealed record LoginRequest(string Email, string Password);
}

public sealed record LoginResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static LoginResult Success() => new(true, []);

    public static LoginResult Failure(IReadOnlyList<string> errors) => new(false, errors);
}
