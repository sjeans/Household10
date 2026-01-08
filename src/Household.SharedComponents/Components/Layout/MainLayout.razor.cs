using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Serilog;

namespace Household.SharedComponents.Components.Layout;

public partial class MainLayout : LayoutComponentBase
{
    ////private bool _darkMode = false;

    ////private void HandleDarkModeToggled(bool isDarkMode)
    ////{
    ////    _darkMode = isDarkMode;
    ////    StateHasChanged();
    ////}

    //private ILogger _logger = default!;

    //public MainLayout(ILogger logger)
    //{
    //    _logger = logger;
    //}
    [Inject] private AuthenticationStateProvider Auth { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;

    private bool isAuthenticated = false;
    private string? username;
    private string? greeting;

    protected override async Task OnInitializedAsync()
    {
        AuthenticationState st = await Auth.GetAuthenticationStateAsync();

        isAuthenticated = st.User.Identity?.IsAuthenticated ?? false;
        username = isAuthenticated ? st.User.Identity?.Name?.Split(' ')[0] : string.Empty;

        Logger.Information("User authenticated: {IsAuthenticated}, Username: {Username}", isAuthenticated, username);

        int hour = DateTime.Now.Hour;

        if (hour >= 5 && hour < 12)
        {
            greeting = "Good morning";
        }
        else if (hour >= 12 && hour < 18)
        {
            greeting = "Good afternoon";
        }
        else
        {
            greeting = "Good evening";
        }


        //try
        //{
        //}
        //catch (Exception ex)
        //{
        //    _logger.Error("Error fetching page referrer: {Message}", ex.Message);
        //    // Log the exception if necessary
        //    //message = $"Error fetching referrer: {ex.Message}";
        //}
    }
}
