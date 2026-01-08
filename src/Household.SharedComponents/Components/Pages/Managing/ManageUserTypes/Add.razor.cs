using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Household.SharedComponents.Components.Shared.Messages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Serilog;
using Alert = Household.SharedComponents.Components.Shared.Modals;

namespace Household.SharedComponents.Components.Pages.Managing.ManageUserTypes;

public partial class Add
{
    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private EditContext EditContextRef { get; set; } = new(new UserType());

    private Loading _loadingIndicator = default!;

    private UserType _addUserType = default!;
    private UserType? _originalUserType = default!;

    private ILogger _logger = default!;
    private Notification _notification = new();
    private Alert.Notification _alertNotification = new();
    private UserIpDto _userIp = default!;
    private HttpClient? _client;

    private string _message = string.Empty;
    private string _canShow = string.Empty;
    private bool _canSave = false;
    private bool _editContextInitialized = false;
    private bool IsEnabled = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _logger = Logger.ForContext<Add>();
        _logger.Information("Initializing edit user.");

        _client = ApiService.HttpClient;

        _userIp = await GetUserIpDetailsAsync();
        _canSave = _userIp.CanSave;
        _canShow = _userIp.CanShow;
        _logger.Information("{msg}", _userIp.LogMessage);

        await _loadingIndicator.ShowAsync();
        await GetUserTypeDetailsAsync();

        if (EditContextRef is not null)
        {
            EditContextRef = new EditContext(_addUserType!);
        }

        if (!_editContextInitialized && EditContextRef is not null)
        {
            EditContextRef.OnFieldChanged += EditContext_OnFieldChanged;
            _editContextInitialized = true;
        }

        ValidationContext validationContext = new(_addUserType!);
        PageHistoryState.AddPageToHistory("/managing/usertypes");

        await _loadingIndicator.HideAsync();
    }

    private Task GetUserTypeDetailsAsync()
    {
        try
        {
            _logger.Information("Setting up original state.");
            _originalUserType = new();

            _addUserType = new();

        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered error retrieving user to edit. Error: {errMsg}", ex.Message);
        }

        return Task.CompletedTask;
    }

    private async Task<UserIpDto> GetUserIpDetailsAsync()
    {
        UserIpDto userIp = new();
        if (_client != null)
        {
            HttpResponseMessage response = await _client.GetAsync("api/UserIpService/GetIpAddress");
            if (response.IsSuccessStatusCode)
            {
                userIp = JsonConvert.DeserializeObject<UserIpDto>(response.Content.ReadAsStringAsync().Result) ?? new();
                _logger.Information("Retrieved ip information.");

                userIp.Visible = string.Empty;
                userIp.DisableButton = string.Empty;
                userIp.CanSave = false;
                userIp.CanShow = string.Empty;
            }
        }
        else
            _logger.Error("Failed to retrieve user IP details.");

        return userIp;
    }

    private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        _logger.Information("The following {type} field {fieldName} was updated by {ip}", e.FieldIdentifier.Model.GetType().Name, e.FieldIdentifier.FieldName, _userIp.IpAddress);
    }

    protected async Task HandleSubmit(EditContext editContext)
    {
        if (_client == null)
        {
            _logger.Error("Cannot make client call to retrieve data!");
            return;
        }

        if (!editContext.Validate())
            return;

        if (editContext.IsModified())
        {
            User updatedUser = (User)editContext.Model;
            bool isJsonEqual = updatedUser.JsonCompare(_originalUserType!);

            if (isJsonEqual) // no changes
            {
                _message = "No changes found!";
                _notification.Show(2, true, _message);
            }
            else
            {
                // changes
                _logger.Information("{ip} is adding user.", _userIp.IpAddress);
                HttpResponseMessage response = await _client.PutAsJsonAsync("api/user/updateuser", updatedUser);

                _message = response.StatusCode.ToString();
                _logger.Information("Update from {ip} success: {success}.", _userIp.IpAddress, response.StatusCode);
                if (response.IsSuccessStatusCode)
                    _notification.Show(1, true, "You have successfully added the user!");

            }
        }
        else
        {
            _message = "No changes found!";
            _notification.Show(2, true, _message);
        }
    }
}
