using System.Globalization;
using Microsoft.JSInterop;

namespace Frontend.Services;

public sealed class SessionService(IJSRuntime jsRuntime) : ISessionService
{
    private const string AccessTokenKey = "controle-financeiro.auth.access-token";
    private const string ExpiresAtKey = "controle-financeiro.auth.expires-at";

    public async Task<bool> StoreAsync(string? accessToken, string? expiresAt)
    {
        if (!IsValid(accessToken, expiresAt))
        {
            return false;
        }

        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", AccessTokenKey, accessToken);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", ExpiresAtKey, expiresAt);
            return true;
        }
        catch
        {
            await ClearAsync();
            return false;
        }
    }

    public async Task<bool> HasValidSessionAsync()
    {
        try
        {
            var accessToken = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", AccessTokenKey);
            var expiresAt = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", ExpiresAtKey);

            if (IsValid(accessToken, expiresAt))
            {
                return true;
            }
        }
        catch
        {
            // The session cannot be trusted when browser storage is unavailable.
        }

        await ClearAsync();
        return false;
    }

    private static bool IsValid(string? accessToken, string? expiresAt) =>
        !string.IsNullOrWhiteSpace(accessToken)
        && DateTimeOffset.TryParse(
            expiresAt,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces,
            out var expiresAtInstant)
        && expiresAtInstant > DateTimeOffset.UtcNow;

    private async Task ClearAsync()
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", AccessTokenKey);
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", ExpiresAtKey);
        }
        catch
        {
            // Cleanup is best-effort when browser storage is unavailable.
        }
    }
}
