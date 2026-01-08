using System.Net.Http.Json;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Modals;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Serilog;
using Alert = Household.SharedComponents.Components.Shared.Messages;

namespace Household.SharedComponents.Components.Pages.AddressBook;

public partial class AddContact
{
    [Parameter]
    public int AddressId { get; set; }

    [Parameter]
    public int Id { get; set; }

    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;

    private string _addressName = default!;
    private string _contactName = default!;
    private ContactNumberDto _newContactNumber = new();
    private string? _message = default!;

    private UserIpDto _userIp = default!;
    private ContactNumberDto? _originalContact;
    private Notification _notification = new();
    private Alert.Notification _alertNotification = new();
    private HttpClient? _client;
    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<AddContact>();
        _logger.Information("Initializing add contact.");
        _client = ApiService.HttpClient;
        _userIp = await GetUserIpDetails();
        await GetContactDetails();
    }

    private async Task GetContactDetails()
    {
        if (_client == null)
            return;

        try
        {
            _logger.Information("Retrieving address to associate contact with.");
            AddressInfoDto? address = await _client.GetFromJsonAsync<AddressInfoDto>($"api/AddressBook/{AddressId}");
            ContactNumberDto? contactNumber = address?.ContactNumbers?.FirstOrDefault(cn => cn.Id == Id);

            if (address != null)
                _addressName = address!.Name;

            if (contactNumber != null)
            {
                _originalContact = new()
                {
                    Id = contactNumber.Id,
                    Name = contactNumber.Name,
                    PhoneNumber = contactNumber.PhoneNumber,
                };
                _newContactNumber = contactNumber;
                _contactName = contactNumber.Name!;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered error adding new contact to address: {AddressName} with error: {errMsg}.", _addressName, ex.GetInnerMessage());
        }

        PageHistoryState.AddPageToHistory($"/addressbook/edit/{AddressId}");
    }

    protected async Task HandleInvalid()
    {
        await base.OnInitializedAsync();
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

        _originalContact ??= new ();

        if (editContext.Validate())
        {
            ContactNumberDto updatedContactNumber = editContext.Model as ContactNumberDto ?? new();

            if (updatedContactNumber.Name.IsNullOrWhiteSpace() && updatedContactNumber.PhoneNumber.IsNullOrWhiteSpace()) // no changes
            {
                _message = "No changes found!";
                _alertNotification.Show(2, true, _message);
            }
            else
            {
                // changes
                _logger.Information("Adding new contact to address {AddressName}.", _addressName);
                HttpResponseMessage response = await _client.PostAsJsonAsync("api/contactnumber/", updatedContactNumber);

                _logger.Information("Add contact was {success}.", response.StatusCode);
                _message = response.StatusCode.ToString();
                if (response.IsSuccessStatusCode)
                {
                    _notification.Show(1, true, "You have successfully added the contact!");
                }
            }
        }
    }
}
