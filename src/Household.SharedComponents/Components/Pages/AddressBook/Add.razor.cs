using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Household.Shared.Dtos;
using Household.Shared.Enums;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Modals;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Serilog;
using Alert = Household.SharedComponents.Components.Shared.Messages;

namespace Household.SharedComponents.Components.Pages.AddressBook;

public partial class Add
{
    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;

    private AddressInfoDto _address = new();
    private string _message = string.Empty;
    private EditContext EditContextRef { get; set; } = default!;

    private UserIpDto _userIp = default!;
    private bool _canSave;
    private string _canShow = default!;
    private string _disable = default!;
    private string _visible = default!;

    private AddressInfoDto _originalAddress = new();
    private Notification _notification = new();
    private Alert.Notification _alertNotification = new();
    private HttpClient? _client;
    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<Add>();
        _logger.Information("Initializing addressbook add.");

        PageHistoryState.AddPageToHistory("addressbook");
        _address = new();
        _client = ApiService.HttpClient;

        GetAddressDetails();

        EditContextRef = new(_address);
        EditContextRef.OnFieldChanged += EditContext_OnFieldChanged;
        ValidationContext validationContext = new(_address);

        _userIp = await GetUserIpDetails();

        _canSave = _userIp.CanSave;
        _canShow = _userIp.CanShow;
        _visible = _userIp.Visible;
        _disable = _userIp.DisableButton;
        _logger.Information("{msg}", _userIp.LogMessage);
    }

    private void GetAddressDetails()
    {
        _logger.Information("Initializing add address.");
        AddressInfoDto? address = _address;

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
            AddressInfoDto updatedAddress = (AddressInfoDto)editContext.Model;
            bool isJsonEqual = updatedAddress.JsonCompare(_originalAddress!);

            if (isJsonEqual) // no changes
            {
                _message = "No changes found!";
                _alertNotification.Show(2, true, _message);
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
                {
                    _notification.Show(1, true, "You have successfully added the address!");
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
