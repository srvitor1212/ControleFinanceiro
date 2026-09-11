using Frontend;
using Frontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var authBaseUrl = builder.Configuration["AuthService:BaseUrl"];
if (!Uri.TryCreate(authBaseUrl, UriKind.Absolute, out var authBaseUri)
    || (authBaseUri.Scheme != Uri.UriSchemeHttp && authBaseUri.Scheme != Uri.UriSchemeHttps))
{
    throw new InvalidOperationException("AuthService:BaseUrl must be an absolute HTTP(S) URI.");
}

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IAuthService>(_ => new AuthService(authBaseUri));
builder.Services.AddScoped<ISessionService, SessionService>();

await builder.Build().RunAsync();
