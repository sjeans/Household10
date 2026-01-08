using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Shared.Buttons;

public partial class SaveButton : ComponentBase
{
    [Parameter]
    public string ButtonText { get; set; } = string.Empty;

    [Parameter]
    public bool CanSave { get; set; }

    [Parameter]
    public string CanShow { get; set; } = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
    }
}
