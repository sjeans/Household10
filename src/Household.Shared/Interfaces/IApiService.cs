using Microsoft.AspNetCore.Components;

namespace Household.Shared.Interfaces;

public interface IApiService
{
    HttpClient HttpClient { get; }
    NavigationManager NavigationManager { get; }
}
