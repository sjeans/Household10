using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Pages;

public partial class Auth_Error
{
    [Parameter, SupplyParameterFromQuery(Name = "code")]
    public string? ErrorCode { get; set; }

    [Parameter, SupplyParameterFromQuery(Name = "description")]
    public string? ErrorDescription { get; set; }

    private void GoHome() => NavigationManager.NavigateTo("/");
}
