using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Blazorise;
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

public partial class Edit
{
    [Parameter]
    public int Id { get; set; }

    [Inject] public IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] public required IApiService ApiService { get; set; }
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private Loading _loadingIndicator = default!;

    private EditContext EditContextRef { get; set; } = new(new User());

    private bool IsEnabled { get; set; } = true;

    private User _editUser = default!;
    //private UserType _editUserType = default!;
    private string? _message = default!;
    private string _username = string.Empty;

    private bool _canSave = false;
    private string _canShow = string.Empty;
    private bool _editContextInitialized = false;

    private User? _originalUser = default!;
    private Notification _notification = new();
    private UserIpDto _userIp = default!;
    private HttpClient? _client;
    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<Edit>();
        _logger.Information("Initializing edit user.");

        _client = ApiService.HttpClient;
        if (_editUser == null)
        {
            _userIp = await GetUserIpDetailsAsync();
            _canSave = _userIp.CanSave;
            _canShow = _userIp.CanShow;
            _logger.Information("{msg}", _userIp.LogMessage);

            await _loadingIndicator.ShowAsync();
            await GetUserDetails();

            EditContextRef ??= new EditContext(_editUser!);

            if (!_editContextInitialized && EditContextRef is not null)
            {
                EditContextRef.OnFieldChanged += EditContext_OnFieldChanged;
                _editContextInitialized = true;
            }

            ValidationContext validationContext = new(_editUser!);
            PageHistoryState.AddPageToHistory("/managing/users");
            await _loadingIndicator.HideAsync();
        }
    }

    private async Task GetUserDetails()
    {
        if (_client == null)
        {
            _logger.Error("Cannot make client call to retrieve data!");
            return;
        }

        try
        {
            _logger.Information("Retrieving user to edit.");
            HttpResponseMessage response = await _client.GetAsync($"api/user/{Id}");

            User user = new ();
            if (response.IsSuccessStatusCode)
            {
                await using Stream stream = response.Content.ReadAsStream(new());
                Result<User> result = await JsonDeserializer.TryDeserializeAsync<User>(stream, new());

                if (!result.IsSuccess)
                {
                    _logger.Error("Failed to deserialize: {msg}", result.Error);
                    return;
                }

                _logger.Information("Retrieved user to edit.");
                user = result.Value!;
            }

            if (user != null)
            {
                _logger.Information("Setting up original state.");
                _originalUser = new()
                {
                    Id = user.Id,
                    Active = user.Active,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Password = user.Password,
                    UserName = user.UserName,
                    UserTypeId = user.UserTypeId,
                    UserType = user.UserType,
                };

                _editUser = user;
                _username = user.FirstName + " " + user.LastName;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered error retrieving user to edit. Error: {errMsg}", ex.Message);
        }
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

    protected void UserTypeUpdate(int newUserType) => UpdateProperty("UserType", newUserType);

    protected void UpdateProperty(string propertyName, int newValue)
    {
        if (propertyName == "UserType")
            _editUser.UserTypeId = newValue;

        EditContextRef.NotifyFieldChanged(EditContextRef.Field(propertyName));
    }

    protected static void ValidateProperty(ValidatorEventArgs eventArgs)
    {
        bool selection = int.TryParse((string?)eventArgs.Value, out _);

        eventArgs.Status = selection ? ValidationStatus.Success : ValidationStatus.Error;
    }

    protected async Task HandleSubmit(EditContext editContext)
    {
        if (_client == null)
            return;

        if (!editContext.Validate())
            return;

        if (editContext.IsModified())
        {
            User updatedUser = (User)editContext.Model;

            if (updatedUser.Id > 0)
            {
                bool isJsonEqual = updatedUser.JsonCompare(_originalUser!);

                if (isJsonEqual) // no changes
                    _message = "No changes found!";
                else
                {
                    // changes
                    _logger.Information("{ip} is updating show.", _userIp.IpAddress);
                    HttpResponseMessage response = await _client.PutAsJsonAsync("api/user/updateuser", updatedUser);

                    _message = response.StatusCode.ToString();
                    _logger.Information("Update from {ip} success: {success}.", _userIp.IpAddress, response.StatusCode);
                    if (response.IsSuccessStatusCode)
                        _notification.Show(1, true, "You have successfully updated the user!");

                }
            }
            else
                _message = "Invalid Role selected!";
        }
        else
            _message = "No changes found!";

    }
}
