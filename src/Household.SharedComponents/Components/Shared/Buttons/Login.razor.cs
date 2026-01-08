using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Shared.Buttons;

public partial class Login
{
    [Inject] NavigationManager Nav { get; set; } = default!;
    private void SignIn() => Nav.NavigateTo($"/Account/Login?returnUrl={Uri.EscapeDataString(Nav.Uri)}", true);
    // /Account/Login?returnUrl=/
}
