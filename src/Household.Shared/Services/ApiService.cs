using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Household.Shared.Services;

/// <inheritdoc/>
public class ApiService : IApiService
{
    /// <inheritdoc/>
    public HttpClient HttpClient { get; }
    /// <inheritdoc/>
    public NavigationManager NavigationManager { get; }

    public ApiService(IHttpClientFactory httpClientFactory, NavigationManager navigationManager)
    {
        HttpClient = httpClientFactory.CreateClient();
        NavigationManager = navigationManager;
        HttpClient.BaseAddress = new Uri(NavigationManager.BaseUri);
        //HttpClient.BaseAddress = new Uri(NavigationManager.BaseUri.Replace("https", "http"));
    }
}
