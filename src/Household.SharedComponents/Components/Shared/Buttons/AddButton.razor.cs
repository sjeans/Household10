using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Shared.Buttons;

public partial class AddButton : ComponentBase
{
    [Parameter]
    public string VisibilityStyle { get; set; } = string.Empty;

    [Parameter]
    public bool Enabled { get; set; }

    [Parameter] 
    public string NavigateToUrl { get; set; } = string.Empty;

    [Parameter]
    public string ButtonText { get; set; } = string.Empty;

    [Inject] protected NavigationManager NavManager { get; set; } = default!;

    protected void Add()
    {
        if (!NavigateToUrl.IsNullOrWhiteSpace())
            NavManager.NavigateTo(NavigateToUrl, true);
    }
}
