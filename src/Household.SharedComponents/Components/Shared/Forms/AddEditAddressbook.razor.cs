using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Household.SharedComponents.Components.Shared.Modals;
using Household.Shared.Dtos;
using Household.Shared.Enums;
using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Shared.Forms;

public partial class AddEditAddressbook
{
    [Parameter]
    public AddressInfoDto Address { get; set; } = new();

    [Parameter]
    public bool CanSave { get; set; }

    [Parameter]
    public string CanShow { get; set; } = default!;

    [Parameter]
    public HttpClient? Client { get; set; }

    [Parameter]
    public ILogger? Log { get; set; }

    [Parameter]
    public Notification ComponentNotification { get; set; } = default!;

    [Parameter]
    public string Message { get; set; } = default!;

    [Parameter]
    public EditContext EditContext { get; set; } = default!;

    private UserIpDto _userIp = default!;
    private string _disable = string.Empty;
    private string _visible = string.Empty;

    private EditContext _editContextRef = default!;
    private AddressInfoDto _originalAddress = new();
    private readonly Notification _notification = new();
    private ILogger _logger = default!;

    private bool _editContextInitialized;

    public AddEditAddressbook(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task OnInitializedAsync()
    {
        // Fix: Create a LoggerFactory instance, then use it to create the logger.
        //using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => { });
        //_logger = loggerFactory.CreateLogger<AddEditAddressbook>();
        _logger.Information("Initializing addressbook add.");
        Address = new();

        GetAddressDetails();

        if (EditContext is null)
        {
            EditContext = new EditContext(Address);
        }

        if (!_editContextInitialized && EditContext is not null)
        {
            EditContext.OnFieldChanged += EditContext_OnFieldChanged;
            _editContextInitialized = true;
        }

        ValidationContext validationContext = new(Address);

        _userIp = await GetUserIpDetails();

        CanSave = _userIp.CanSave;
        CanShow = _userIp.CanShow;
        _visible = _userIp.Visible;
        _disable = _userIp.DisableButton;
        _logger.Information("{msg}", _userIp.LogMessage);
    }

    private void GetAddressDetails()
    {
        _logger.Information("Initializing address add or edit component.");
        AddressInfoDto? address = Address;

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
        Address.State = ((States)selectedState).ToString();
        _editContextRef.NotifyFieldChanged(_editContextRef.Field("State"));
    }

    private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        _logger.Information("The following {type} field {fieldName} was updated by {ip}", e.FieldIdentifier.Model.GetType().Name, e.FieldIdentifier.FieldName, _userIp.IpAddress);
    }

    private async Task<UserIpDto> GetUserIpDetails()
    {
        UserIpDto userIp = default!;
        if (Client != null)
        {
            HttpResponseMessage response = await Client.GetAsync("api/UserIpService/GetIpAddress");
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
        if (Client == null)
            return;

        bool isValid = editContext.Validate();

        if (!isValid)
            return;

        if (editContext.IsModified())
        {
            AddressInfoDto updatedAddress = (AddressInfoDto)editContext.Model;
            bool isJsonEqual = updatedAddress.JsonCompare(_originalAddress!);

            if (isJsonEqual) // no changes
                Message = "No changes found!";
            else
            {
                // changes
                _logger.Warning("{ip} is adding an address.", _userIp.IpAddress);
                string baseUrl = "api/addressbook/";
                HttpResponseMessage response = await Client!.PostAsJsonAsync(baseUrl, updatedAddress);

                _logger.Warning("Add from {ip} for address was {success}.", _userIp.IpAddress, response.StatusCode);
                Message = response.StatusCode.ToString();
                if (response.IsSuccessStatusCode)
                    _notification.Show(1, true, "You have successfully added the address!");

            }
        }
        else
            Message = "No changes found!";

    }
}
