using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend;
using Frontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
var authServiceBaseUrl = builder.Configuration["AuthService:BaseUrl"]
    ?? throw new InvalidOperationException("A configuração AuthService:BaseUrl não foi definida.");
builder.Services.AddScoped(_ => new AuthServiceClient(new HttpClient
{
    BaseAddress = new Uri(authServiceBaseUrl, UriKind.Absolute)
}));

await builder.Build().RunAsync();
