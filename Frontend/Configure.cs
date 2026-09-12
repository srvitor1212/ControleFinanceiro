using Frontend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Frontend;

public static class Configure
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string hostBaseAddress)
    {
        var authBaseUrl = configuration["AuthService:BaseUrl"];
        if (!Uri.TryCreate(authBaseUrl, UriKind.Absolute, out var authBaseUri)
            || (authBaseUri.Scheme != Uri.UriSchemeHttp && authBaseUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("AuthService:BaseUrl must be an absolute HTTP(S) URI.");
        }

        services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(hostBaseAddress) });
        services.AddScoped<IAuthService>(_ => new AuthService(authBaseUri));
        services.AddScoped<ISessionService, SessionService>();

        return services;
    }
}
