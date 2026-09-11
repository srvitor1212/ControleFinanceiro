namespace Frontend.Services;

public interface ISessionService
{
    Task<bool> StoreAsync(string? accessToken, string? expiresAt);

    Task<bool> HasValidSessionAsync();
}
