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

public partial class Edit : ComponentBase
{
    [Parameter]
    public int Id { get; set; }

    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private EditContext EditContextRef { get; set; } = new(new UserType());

    private UserType _editUserType = default!;
    private UserType? _originalUserType = default!;

    private Loading _loadingIndicator = default!;
    private ILogger _logger = default!;
    private Notification _notification = new();
    private Alert.Notification _alertNotification = new();
    private UserIpDto _userIp = default!;
    private HttpClient? _client;

    private string? _message = default!;
    private string _roleName = string.Empty;
    private string _canShow = string.Empty;
    private bool _canSave = false;
    private bool _editContextInitialized = false;
    private bool IsEnabled = true;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _logger = Logger.ForContext<Edit>();
        _logger.Information("Initializing edit user.");

        _client = ApiService.HttpClient;

        if (Id > 0)
        {
            _userIp = await GetUserIpDetailsAsync();
            _canSave = _userIp.CanSave;
            _canShow = _userIp.CanShow;
            _logger.Information("{msg}", _userIp.LogMessage);

            await GetUserTypeDetails();

            if (EditContextRef is not null)
            {
                EditContextRef = new EditContext(_editUserType!);
            }

            if (!_editContextInitialized && EditContextRef is not null)
            {
                EditContextRef.OnFieldChanged += EditContext_OnFieldChanged;
                _editContextInitialized = true;
            }

            ValidationContext validationContext = new(_editUserType!);
            PageHistoryState.AddPageToHistory("/managing/usertypes");
        }
    }

    private async Task GetUserTypeDetails()
    {
        if (_client == null)
        {
            _logger.Error("Cannot make client call to retrieve data!");
            return;
        }

        await _loadingIndicator.ShowAsync();
        
        try
        {
            _logger.Information("Retrieving user to edit.");
            HttpResponseMessage response = await _client.GetAsync($"api/usertype/{Id}");

            UserType userType = new();
            if (response.IsSuccessStatusCode)
            {
                await using Stream stream = response.Content.ReadAsStream(new());
                Result<UserType> result = await JsonDeserializer.TryDeserializeAsync<UserType>(stream, new());

                if (!result.IsSuccess)
                {
                    _logger.Error("Failed to deserialize: {msg}", result.Error);
                    return;
                }

                _logger.Information("Retrieved user to edit.");
                userType = result.Value!;

                _editUserType = userType;
                _roleName = userType.Description ?? string.Empty;
            }

            if (userType != null)
            {
                _logger.Information("Setting up original state.");
                _originalUserType = new()
                {
                    Id = userType.Id,
                    Description = userType.Description,
                    Location = userType.Location,
                    User = userType.User,
                };
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered error retrieving user to edit. Error: {errMsg}", ex.Message);
        }

        await _loadingIndicator.HideAsync();
        return;
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
            _message = "Cannot make client call to retreive data!";
            _notification.Show(3, true, _message);
            return;
        }

        if (!editContext.Validate())
        {
            _message = string.Join($"{Environment.NewLine}", editContext.GetValidationMessages());
            _notification.Show(3, true, _message);
            return;
        }

        if (editContext.IsModified())
        {
            AddressInfoDto updatedAddress = (AddressInfoDto)editContext.Model;
            bool isJsonEqual = updatedAddress.JsonCompare(_originalUserType!);

            if (isJsonEqual) // no changes
            {    
                _message = "No changes found!";
                _notification.Show(2, true, _message);
            }
            else
            {
                // changes
                _logger.Warning("{ip} is adding an address.", _userIp.IpAddress);
                string baseUrl = "api/addressbook/";
                HttpResponseMessage response = await _client!.PostAsJsonAsync(baseUrl, updatedAddress);

                _logger.Warning("Add from {ip} for address was {success}.", _userIp.IpAddress, response.StatusCode);
                _message = response.StatusCode.ToString();
                if (response.IsSuccessStatusCode)
                    _notification.Show(1, true, "You have successfully added the address!");

            }
        }
        else
        {
            _message = "No changes found!";
            _notification.Show(2, true, _message);
        }
    }
}
