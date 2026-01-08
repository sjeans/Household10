using System.ComponentModel;
using Blazorise;
using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Household.SharedComponents.Components.Shared.Modals;
using Serilog;

namespace Household.SharedComponents.Components.Shared.Buttons;

public partial class DeleteButton : ComponentBase
{
    [CascadingParameter]
    public bool IsEnabled { get; set; }

    [Parameter]
    public string ActionType { get; set; } = string.Empty;

    [Parameter]
    public string ButtonText { get; set; } = string.Empty;

    [Parameter]
    public string DeletedItemName { get; set; } = string.Empty;

    [Parameter]
    public int DeletedItemId { get; set; }

    [Parameter]
    public bool CanSave { get; set; }

    [Parameter]
    public string CanShow { get; set; } = string.Empty;

    [Parameter]
    public HttpClient Client { get; set; } = default!;

    [Parameter]
    public string ClientId { get; set; } = string.Empty;

    [Parameter]
    public ILogger? Logger { get; set; }

    [Parameter]
    public string Message { get; set; } = string.Empty;

    [Parameter]
    public Notification Notification { get; set; } = new();

    [Parameter]
    public string URI { get; set; } = string.Empty;

    [Parameter]
    public EventCallback OnDeleted { get; set; }

    [Parameter]
    public string NotificationTitle { get; set; } = string.Empty;

    private Modal? _confirmModal;
    private bool _isDeleting;
    private bool _modalVisible = false;
    private string _buttonClass = "m-sm-2";

    protected override async Task OnParametersSetAsync()
    {
        if (!IsEnabled)
            _buttonClass = "m-sm-2 d-none";

        await base.OnParametersSetAsync();
    }

    private async Task CloseModal()
    {
        _modalVisible = false;

        await _confirmModal!.Hide();
        return;
    }

    private async Task ShowModal()
    {
        _modalVisible = true;
        await _confirmModal!.Show();
        return;
    }

    protected async Task Delete()
    {
        if (Client == null)
        {
            Logger?.Warning("Cannot make client calls for data!");
            return;
        }

        if (string.IsNullOrWhiteSpace(URI))
        {
            Logger?.Warning("URI is null or empty.");
            return;
        }

        _isDeleting = true;

        try
        {
            Logger?.Information("Deleting {name}.", DeletedItemName);

            //HttpResponseMessage response = await Client.DeleteAsync(URI);
            //string responseString = await response.Content.ReadAsStringAsync();

            //Logger?.Information("Deleted: {status}.", response.IsSuccessStatusCode);
            //Message = response.StatusCode.ToString();

            //if (response.IsSuccessStatusCode)
            //{
            Logger?.Information("You have successfully deleted {name}!", DeletedItemName);
            Notification.Show(1, true, $"You have successfully deleted {DeletedItemName}!");
            //}
            //else
            //{
            //    Logger?.Error("{msg}", responseString);
            //    Notification.Show(2, false, $"{Message}");
            //}
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Encountered an error deleting {name}. Error: {errMsg}", DeletedItemName, ex.GetInnerMessage());
            Notification.Show(3, false, $"Error deleting {DeletedItemName}: {ex.Message}");
        }
        finally
        {
            _isDeleting = false;
            _confirmModal?.Hide();
        }
    }

    private Task OnModalClosing(CancelEventArgs e)
    {
        CloseReason closeReasonEnum = ((ModalClosingEventArgs)e).CloseReason;
        if (closeReasonEnum != CloseReason.UserClosing)
            e.Cancel = true;

        return Task.CompletedTask;
    }

    protected async Task DeleteSubscription()
    {
        if (Client == null)
        {
            Logger?.Warning("Cannot make client calls for data!");
            return;
        }

        try
        {
            Logger?.Information("Deleting the subscription {name}.", DeletedItemName);

            HttpResponseMessage response = await Client.DeleteAsync($"api/subscriptions/removesubscription/{DeletedItemId}");
            string responseString = await response.Content.ReadAsStringAsync();

            Logger?.Information("Deleted subscription: {status}.", response.IsSuccessStatusCode);
            Message = response.StatusCode.ToString();

            if (response.IsSuccessStatusCode)
            {
                Logger?.Information("You have successfully deleted the subscription {name}!", DeletedItemName);
                Notification.Show(1, true, "You have successfully deleted the service!");
            }
            else
            {
                Logger?.Error("{msg}", responseString);
                Notification.Show(2, false, $"{Message}");
            }
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Encountered an error deleting show. Error: {errMsg}", ex.GetInnerMessage());
        }
    }

    protected async Task DeleteShow()
    {
        if (Client == null)
        {
            Logger?.Warning("Cannot make client calls for data!");
            return;
        }

        try
        {
            Logger?.Information("{ip} is deleting the show {name}.", ClientId, DeletedItemName);

            HttpResponseMessage response = await Client.DeleteAsync($"api/shows/removeshow/{DeletedItemId}");
            string responseString = await response.Content.ReadAsStringAsync();

            Logger?.Information("{ip} is deleted show: {status}.", ClientId, response.IsSuccessStatusCode);
            Message = response.StatusCode.ToString();

            if (response.IsSuccessStatusCode)
            {
                Notification?.Show(1, true, $"You have successfully deleted the show {DeletedItemName}!");
                ActionType = "deleted";
            }
            else
                Logger?.Information("{msg}", responseString);


        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "{ip} encountered an error deleting show. Error: {errMsg}", ClientId, ex.GetInnerMessage());
        }
    }
}
