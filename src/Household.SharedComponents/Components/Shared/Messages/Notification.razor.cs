using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Household.SharedComponents.Components.Shared.Messages;

public partial class Notification
{
    [Inject] IJSRuntime JsRuntime { get; set; } = default!;

    [Parameter]
    public string? Message { get; set; }

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public bool Visible { get; set; }

    private Alert _alert = default!;
    private bool _visible = false;
    private Color _color = Color.Primary;
    
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
    }

    private void SetHeader(int type, bool hide)
    {
        _visible = hide;
        switch (type)
        {
            case 1:
                Title = "Success";
                //_background = "background-color: #CFEDE0; color: #227254; border-color: #97D7BC; border-radius: 1rem 1rem 0 0; font-weight: 600 !important;";
                _color = Color.Success;
                break;
            case 2:
                Title = "Warning";
                //_background = "background-color: #FCF8E3; color: #000; border-color: #FFC107; border-radius: 1rem 1rem 0 0; font-weight: 600 !important;";
                _color = Color.Warning;
                break;
            case 3:
                Title = "Error";
                //_background = "background-color: #DC3545; color: #FFFFFF; border-color: #A82431; border-radius: 1rem 1rem 0 0; font-weight: 600 !important;";
                _color = Color.Danger;
                break;
            default:
                Title = "Notification";
                //_background = "background-color: var(--bs-info-bg-subtle); color: var(--bs-info-text-emphasis); border-color: var(--bs-info-border-subtle); border-radius: 1rem 1rem 0 0; font-weight: 600 !important;";
                _color = Color.Info;
                break;
        }
    }

    /// <summary>
    /// Shows the notification with the specified type, visibility, and message.
    /// </summary>
    /// <param name="notificationType">Success, Warning, Error, and Infomational</param>
    /// <param name="visible">Display or not</param>
    /// <param name="message">Message to display</param>
    public async void Show(int notificationType, bool visible, string message)
    {
        Visible = visible;
        SetHeader(notificationType, visible);
        Message = message;
        await JsRuntime.InvokeVoidAsync("scrollToTop");
        StateHasChanged();
    }
}
