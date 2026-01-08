using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using System.Timers;

namespace Household.SharedComponents.Components.Shared;

public partial class SuccessNotification : ComponentBase
{
    [Parameter]
    public string? Parent { get; set; }
    
    [Parameter]
    public string? Message { get; set; }
    
    [Parameter]
    public string? TypeOfAction { get; set; }

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public bool AutoHide { get; set; } = true;

    [Inject] public NavigationManager? Navigation { get; set; }

    private string? _modalDisplay;
    private string? _modalClass;
    private bool _showBackdrop;
    private System.Timers.Timer? _countdown;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        _modalDisplay = "none;";
        _modalClass = "";
        _showBackdrop = false;
    }

    public void Show(string message)
    {
        StartCountdown();
        Message = message;
        _modalDisplay = "block;";
        _modalClass = "show";
        _showBackdrop = true;
    }

    private void Hide()
    {
        _modalDisplay = "none;";
        _modalClass = "";
        _showBackdrop = false;

        if (Parent.IsNullOrWhiteSpace()) { Parent = ""; }
        if (Parent.ToLower().Contains("user"))
            Navigation?.NavigateTo($"/{Parent}");
        else if (Parent.ToLower().Contains("showdetail"))
            Navigation?.NavigateTo($"/{Parent}");
        else
            Navigation?.NavigateTo($"/{Parent}/index");

    }

    private void StartCountdown()
    {
        SetCountdown();

        if (_countdown!.Enabled)
        {
            _countdown!.Start();
        }
        else
        {
            _countdown.Start();
            _countdown.Stop();
        }
    }

    private void SetCountdown()
    {
        if (_countdown != null) return;

        _countdown = new System.Timers.Timer(5000);
        _countdown.Elapsed += OnHide;
        _countdown.Enabled = AutoHide;
        _countdown.AutoReset = false;
    }

    private void OnHide(object? sender, ElapsedEventArgs e)
    {
        Hide();
    }
}
