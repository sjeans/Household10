using Microsoft.AspNetCore.Components;

namespace Household.Shared.Services.Interfaces;

/// <summary>
/// Provides access to core services for making HTTP requests and handling navigation within an application.
/// </summary>
/// <remarks>This interface is typically used to abstract dependencies on HTTP communication and navigation logic,
/// allowing for easier testing and separation of concerns. Implementations should ensure that the provided services are
/// properly configured for the application's environment.</remarks>
public interface IApiService
{
    /// <summary>
    /// Creates and configures an instance of <see cref="HttpClient"/> for making HTTP requests. Not used for authenticated requests.
    /// </summary>
    HttpClient HttpClient { get; }
    /// <summary>
    /// Gets the navigation manager used to programmatically navigate and manage URI state within the application.
    /// </summary>
    /// <remarks>Use this property to perform navigation operations, query the current URI, or handle
    /// navigation events. The navigation manager provides methods for navigating to new pages and for responding to
    /// changes in the application's location.</remarks>
    NavigationManager NavigationManager { get; }
}
