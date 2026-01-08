using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Serilog;

namespace Household.SharedComponents.Components.Shared.Buttons;

public partial class Authentication : ComponentBase
{
    [Inject] NavigationManager NavMan { get; set; } = default!;
    [Inject] AuthenticationStateProvider Auth { get; set; } = default!;

    private bool isAuthenticated;
    private string pageReferrer = "/";
    private readonly ILogger _log = Log.ForContext<Authentication>();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            AuthenticationState st = await Auth.GetAuthenticationStateAsync();

            isAuthenticated = st.User.Identity?.IsAuthenticated ?? false;
            pageReferrer = NavMan.Uri;
            _log.Information("User is authenticated: {IsAuthenticated}", isAuthenticated);
        }
        catch (Exception ex)
        {
            _log.Error("Error fetching page referrer: {Message}", ex.Message);
            // Log the exception if necessary
            //message = $"Error fetching referrer: {ex.Message}";
        }
    }
}
