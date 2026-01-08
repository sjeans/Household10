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

namespace Household.SharedComponents.Components.Pages.Managing.ManageUsers;

public partial class Add : ComponentBase
{

    [Inject] public IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] public required IApiService ApiService { get; set; }
    [Inject] private ILogger Logger { get; set; } = default!;

    private EditContext _editContextRef = new(new User());
    private Loading _loadingIndicator = default!;
    private User _addUser = new();
    private User _originalUser = new();

    private Notification _notification = new();
    private UserIpDto _userIp = default!;
    private HttpClient? _client;
    private ILogger _logger = default!;

    private string? _message = default!;
    private string _canShow = string.Empty;
    private string _clientId = string.Empty;
    private bool _canSave = false;
    private bool _editContextInitialized = false;
    private bool IsEnabled = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _logger = Logger.ForContext<Add>();
        _logger.Information("Initializing add user.");
        _client = ApiService.HttpClient;
        _userIp = await GetUserIpDetailsAsync();
        _canSave = _userIp.CanSave;
        _canShow = _userIp.CanShow;
        _clientId = _userIp.IpAddress;
        await GetUserDetails();

        _editContextRef ??= new EditContext(_addUser!);

        if (!_editContextInitialized && _editContextRef is not null)
        {
            _editContextRef.OnFieldChanged += EditContext_OnFieldChanged;
            _editContextInitialized = true;
        }

        ValidationContext validationContext = new(_addUser!);
    }

    private async Task GetUserDetails()
    {
        await _loadingIndicator.ShowAsync();
        
        if (_addUser != null)
        {
            _logger.Information("Setting up original state.");
            _originalUser = new()
            {
                Id = _addUser.Id,
                Active = _addUser.Active,
                Email = _addUser.Email,
                FirstName = _addUser.FirstName,
                LastName = _addUser.LastName,
                Password = _addUser.Password,
                UserName = _addUser.UserName,
                UserTypeId = _addUser.UserTypeId,
                UserType = _addUser.UserType,
            };

            //_editContextRef = new(_addUser);
        }

        await _loadingIndicator.HideAsync();
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
            bool isJsonEqual = updatedUser.JsonCompare(_originalUser!);

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
