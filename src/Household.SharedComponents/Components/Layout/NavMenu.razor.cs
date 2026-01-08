using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Serilog;

namespace Household.SharedComponents.Components.Layout;

public partial class NavMenu : ComponentBase
{
    [Inject] private AuthenticationStateProvider Auth { get; set; } = default!;
    [Inject] NavigationManager NavMan { get; set; } = default!;

    private ILogger _logger = default!;

    private bool collapseNavMenu = false;

    private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;

    private bool isAuthenticated;
    private string? pageReferrer;

    public NavMenu(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task OnInitializedAsync()
    {
        AuthenticationState st = await Auth.GetAuthenticationStateAsync();

        isAuthenticated = st.User.Identity?.IsAuthenticated ?? false;
        pageReferrer = Uri.EscapeDataString(NavMan.Uri);
    }

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }
}
