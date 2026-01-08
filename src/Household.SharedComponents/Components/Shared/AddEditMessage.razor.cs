using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Shared;

public partial class AddEditMessage : ComponentBase
{
    [Parameter]
    public string Message { get; set; } = string.Empty;

    [Parameter]
    public string? Type { get; set; }

    [Parameter]
    public string TypeOfAction { get; set; } = string.Empty;

    private string _alertSuccess = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        HandleMessage();
    }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        if (parameters.TryGetValue(nameof(Message), out string? newMessage))
        {
            Message = newMessage ?? string.Empty;
            HandleMessage();
        }

        return Task.CompletedTask;
    }

    protected void HandleMessage()
    {
        switch (Message)
        {
            case null:
            case "":
                break;
            case "OK":
                Message = $"You have successfully {TypeOfAction} the {Type}!";
                _alertSuccess = "alert-success";
                break;
            case "No changes found!":
            case "Date must be later than todays date":
                _alertSuccess = "alert-warning";
                break;
            default:
                Message = "There was an issue with form submission!";
                _alertSuccess = "alert-danger";
                break;
        }

        StateHasChanged();
    }
}
