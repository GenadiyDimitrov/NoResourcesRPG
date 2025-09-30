using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NoResourcesRPG.Client;
using NoResourcesRPG.Client.Helpers;
using NoResourcesRPG.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
// token storage and auth
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

// Register your custom handler
builder.Services.AddScoped<AuthorizationMessageHandler>();

// Register a named HttpClient for talking to your API
builder.Services.AddHttpClient("ServerAPI", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

// Register the default HttpClient that the app will use
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI"));

// Register services for DI
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<CanvasService>();
builder.Services.AddScoped<ICharacterService, CharacterService>();

builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
