////using Microsoft.AspNetCore.Components.Web;
////using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

////namespace Household.Client;

////public class Program
////{
////    public static async Task Main(string[] args)
////    {
////        var builder = WebAssemblyHostBuilder.CreateDefault(args);

////        // Register the root component of your application.
////        // This is the starting point for the client-side UI.
////        builder.RootComponents.Add<App>("#app");
////        builder.RootComponents.Add<HeadOutlet>("head::after");

////        // Configure services for Dependency Injection (DI).
////        // For a typical client, you register an HttpClient for API calls.
////        builder.Services.AddScoped(sp => new HttpClient
////        {
////            BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
////        });

////        // Add any other client-side services here (e.g., specific services, auth providers)

////        await builder.Build().RunAsync();
////    }
////}

//using Microsoft.AspNetCore.Components.Web;
//using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
//using SharedComponents.Components;

//WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder.RootComponents.Add<App>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//builder.Services.AddOidcAuthentication(options =>
//{
//    // Bind the "Keycloak" section from configuration to the ProviderOptions
//    builder.Configuration.Bind("Keycloak", options.ProviderOptions);

//    // Keycloak typically uses the "code" response type by default
//    options.ProviderOptions.ResponseType = "code";

//    // Optional: Add specific scopes if required by your API
//    // options.ProviderOptions.DefaultScopes.Add("api://your-api-scope");

//})/*.AddAccountClaimsPrincipalFactory<CustomAccountFactory>()*/; // Optional: Use a custom factory for role mapping


//// Use the server authentication state provider that reads the server cookie
////builder.Services.AddAuthorizationCore();
////builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

//await builder.Build().RunAsync();

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

await builder.Build().RunAsync();
