using System.ComponentModel;
using System.Timers;
using Blazorise;
using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Shared.Modals;

public partial class Notification : ComponentBase
{
    [Parameter]
    public string? Parent { get; set; }

    [Parameter]
    public string? Message { get; set; } = string.Empty;

    [Inject] public NavigationManager? Navigation { get; set; }

    private Modal? _nofification;
    private System.Timers.Timer? _countdown;
    private string _title = string.Empty;
    private bool _autoHide = true;
    private string? _background;

    public void Show(int type, bool hide, string message)
    {
        SetHeader(type, hide);
        _nofification!.Show();

        StartCountdown();
        Message = message;
        StateHasChanged();
    }

    private void StartCountdown()
    {
        SetCountdown();

        if (!_countdown!.Enabled)
        {
            _countdown.Stop();
            //Countdown.Start();
        }
        else
        {
            _countdown!.Start();
        }
    }

    private void SetCountdown()
    {
        if (_countdown != null) return;

        _countdown = new System.Timers.Timer(5000);
        _countdown.Enabled = _autoHide;
        _countdown.Elapsed += OnHide;
        _countdown.AutoReset = false;
    }

    private void SetHeader(int type, bool hide)
    {
        _autoHide = hide;
        switch (type)
        {
            case 1:
                _title = "Success";
                _background = "background-color: #CFEDE0; color: #227254; border-color: #97D7BC; border-radius: 1rem 1rem 0 0; font-weight: 600 !important;";
                break;
            case 2:
                _title = "Warning";
                _background = "background-color: #FCF8E3; color: #000; border-color: #FFC107; border-radius: 1rem 1rem 0 0; font-weight: 600 !important;";
                break;
            case 3:
                _title = "Error";
                _background = "background-color: #DC3545; color: #FFFFFF; border-color: #A82431; border-radius: 1rem 1rem 0 0; font-weight: 600 !important;";
                break;
            default:
                _title = "Notification";
                _background = "background-color: var(--bs-info-bg-subtle); color: var(--bs-info-text-emphasis); border-color: var(--bs-info-border-subtle); border-radius: 1rem 1rem 0 0; font-weight: 600 !important;";
                break;
        }
    }

    private void OnHide(object? sender, ElapsedEventArgs e)
    {
        _nofification!.Hide();
        if (Parent.IsNullOrWhiteSpace()) { Parent = ""; }
        if (Parent.ToLower().Contains("user"))
            Navigation?.NavigateTo($"/{Parent}");
        else if (Parent.ToLower().Contains("showdetail"))
            Navigation?.NavigateTo($"/{Parent}");
        else
            Navigation?.NavigateTo($"/{Parent}/index");
    }

    private Task OnModalClosing(CancelEventArgs e)
    {
        CloseReason closeReasonEnum = ((ModalClosingEventArgs)e).CloseReason;
        if (closeReasonEnum != CloseReason.UserClosing && !_autoHide)
            e.Cancel = true;

        return Task.CompletedTask;
    }
}
