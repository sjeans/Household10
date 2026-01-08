using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Household.Shared.Dtos;
using Household.Shared.Enums;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Household.SharedComponents.Components.Shared.Modals;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Serilog;
using Alert = Household.SharedComponents.Components.Shared.Messages;

namespace Household.SharedComponents.Components.Pages.AddressBook;

public partial class Edit
{
    [Parameter]
    public int Id { get; set; }

    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    [Inject] private AuthenticationStateProvider Auth { get; set; } = default!;

    private string _addressName = string.Empty;
    private AddressInfoEditDto _address = new();
    private string? _message = default!;
    private EditContext EditContextRef { get; set; } = default!;

    private bool _canSave = false;
    private string _canShow = default!;
    private string _disable = default!;
    private string _visible = default!;

    private Loading _loadingIndicator = default!;

    private UserIpDto _userIp = default!;
    private AddressInfoEditDto _originalAddress = default!;
    private Notification _notification = new();
    private Alert.Notification _alertNotification = new();
    private HttpClient? _client;
    private ILogger _logger = default!;

    private bool _editContextInitialized;

    protected override async Task OnParametersSetAsync()
    {
        _logger = Logger.ForContext<Edit>();
        _logger.Information("Initializing edit addressbook.");
        _client = ApiService.HttpClient;

        AuthenticationState st = await Auth.GetAuthenticationStateAsync();

        bool isAuthenticated = st.User.Identity?.IsAuthenticated ?? false;

        if (isAuthenticated)
        {
            if (Id > 0)
            {
                _userIp = await GetUserIpDetails();

                _canShow = _userIp.CanShow;
                _canSave = _userIp.CanSave;
                _visible = _userIp.Visible;
                _disable = _userIp.DisableButton;
                _logger.Information("{msg}", _userIp.LogMessage);

                await _loadingIndicator.ShowAsync();
                await GetAddressDetails();
                await _loadingIndicator.HideAsync();

                if (EditContextRef is null)
                {
                    EditContextRef = new EditContext(_address);
                }

                if (!_editContextInitialized && EditContextRef is not null)
                {
                    EditContextRef.OnFieldChanged += EditContext_OnFieldChanged;
                    _editContextInitialized = true;
                }

                ValidationContext validationContext = new(_address);
            }

            PageHistoryState.AddPageToHistory("addressbook");
        }
    }

    private async Task GetAddressDetails()
    {
        if (_client == null)
            return;

        try
        {
            _logger.Information("{ip} is retrieving an addressbook entry to edit.", _userIp.IpAddress);
            HttpResponseMessage responseMessage = await _client.GetAsync($"api/addressbook/{Id}");

            responseMessage.EnsureSuccessStatusCode();

            await using Stream stream = responseMessage.Content.ReadAsStream(new());
            Result<AddressInfoEditDto> result = await JsonDeserializer.TryDeserializeAsync<AddressInfoEditDto>(stream, new());

            if (!result.IsSuccess)
            {
                _logger.Error("Failed to deserialize: {msg}", result.Error);
                return;
            }

            AddressInfoEditDto? address = result.Value;

            if (address != null)
            {
                _originalAddress = new()
                {
                    Address = address.Address,
                    Address2 = address.Address2,
                    City = address.City,
                    ContactNumbers = address.ContactNumbers,
                    CountryCode = address.CountryCode,
                    Id = address.Id,
                    Name = address.Name,
                    PostalCode = address.PostalCode,
                    State = address.State,
                };
                _address = address;
                _addressName = address.Name;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error adding addressbook entry! Error: {errMsg}.", ex.Message);
        }
    }

    protected void StatesUpdate(int selectedState)
    {
        _address.State = ((States)selectedState).ToString();
        EditContextRef.NotifyFieldChanged(EditContextRef.Field("State"));
    }

    private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        _logger.Information("The following {type} field {fieldName} was updated by {ip}", e.FieldIdentifier.Model.GetType().Name, e.FieldIdentifier.FieldName, _userIp.IpAddress);
    }

    private async Task<UserIpDto> GetUserIpDetails()
    {
        UserIpDto userIp = default!;
        if (_client != null)
        {
            HttpResponseMessage response = await _client.GetAsync("api/UserIpService/GetIpAddress");
            if (response.IsSuccessStatusCode)
            {
                userIp = JsonConvert.DeserializeObject<UserIpDto>(await response.Content.ReadAsStringAsync()) ?? new();
                _logger.Information("Retrieved ip information.");

                userIp.Visible = string.Empty;
                userIp.DisableButton = string.Empty;
            }
        }
        else
            _logger.Error("Failed to retrieve user IP details.");

        return userIp;
    }

    protected async Task HandleSubmit(EditContext editContext)
    {
        if (_client == null)
            return;

        if (!editContext.Validate())
            return;

        if (editContext.IsModified())
        {
            AddressInfoEditDto? updatedAddress = editContext.Model as AddressInfoEditDto;
            bool isJsonEqual = updatedAddress!.JsonCompare(_originalAddress!);

            if (isJsonEqual) // no changes
            {
                _message = "No changes found!";
                _alertNotification.Show(2, true, _message);
            }
            else
            {
                // changes
                _logger.Information("{ip} is updating an addressbook entry.", _userIp.IpAddress);
                HttpResponseMessage response = await _client.PutAsJsonAsync("api/addressbook/update", updatedAddress);
                _logger.Information("Update from {ip} for addressbook entry was {success}.", _userIp.IpAddress, response.StatusCode);

                _message = response.StatusCode.ToString();
                if (response.IsSuccessStatusCode)
                {
                    _notification.Show(1, true, "You have successfully updated the address!");
                }
            }
        }
        else
        {
            _message = "No changes found!";
            _alertNotification.Show(2, true, _message);
        }
    }
}
