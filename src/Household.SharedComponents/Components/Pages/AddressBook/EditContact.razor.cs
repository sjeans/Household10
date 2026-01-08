using System.Net.Http.Json;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Household.SharedComponents.Components.Shared.Modals;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Serilog;
using Alert = Household.SharedComponents.Components.Shared.Messages;

namespace Household.SharedComponents.Components.Pages.AddressBook;

public partial class EditContact
{
    [Parameter]
    public int AddressId { get; set; }

    [Parameter]
    public int Id { get; set; }

    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    public string AddressName { get; private set; }
    public string ContactName { get; private set; }
    public ContactNumberDto ContactNumber { get; private set; }
    public string? Message { get; private set; }
    
    Loading MyIndicator = default!;

    private UserIpDto _userIp = default!;
    private ContactNumberDto _originalContact = default!;
    private Notification _notification = new();
    private Alert.Notification _alertNotification = new();
    private HttpClient? _client;
    private ILogger _logger = default!;

    public EditContact()
    {
        Id = 0;
        AddressName = string.Empty;
        ContactName = string.Empty;
        ContactNumber = new();
        Message = string.Empty;
    }

    public EditContact(int addressId, int id)
    {
        AddressId = addressId;
        Id = id;
        AddressName = string.Empty;
        ContactName = string.Empty;
        ContactNumber = new();
    }

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<EditContact>();
        _logger.Information("Initializing edit contact");
        _client = ApiService.HttpClient;
        //PageHistoryState.AddPageToHistory("/addressbook/index");
        _userIp = await GetUserIpDetails();

        await MyIndicator.ShowAsync();
        //Visible = userIpService.Visible;
        //Disable = userIpService.DisableButton;
        _logger.Information("{msg}", _userIp.LogMessage);

        await GetContactDetails();
        await MyIndicator.HideAsync();
    }

    private async Task GetContactDetails()
    {
        if (_client == null)
            return;

        try
        {
            _logger.Information("Retrieving address associated with contact.");

            HttpResponseMessage response = await _client.GetAsync($"api/addressbook/{AddressId}");

            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync(new());
            Result<AddressInfoDto> result = await JsonDeserializer.TryDeserializeAsync<AddressInfoDto>(stream, new());

            if (!result.IsSuccess)
            {
                _logger.Error($"Failed to deserialize: {result.Error}");
            }

            AddressInfoDto? address = result.Value;
            ContactNumberDto? contactNumber = address?.ContactNumbers?.FirstOrDefault(cn => cn.Id == Id);

            if (address is not null)
                AddressName = address!.Name;

            if (contactNumber is not null)
            {
                _originalContact = new()
                {
                    Id = contactNumber.Id,
                    Name = contactNumber.Name,
                    PhoneNumber = contactNumber.PhoneNumber,
                };
                ContactNumber = contactNumber;
                ContactName = contactNumber.Name!;
            }

            PageHistoryState.AddPageToHistory($"addressbook/edit/{Id}");
        }
        catch (HttpRequestException hrex)
        {
            _logger.Error(hrex, hrex.GetInnerMessage());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered error updating contact for {AddressName}. With error {errMsg}", AddressName, ex.GetInnerMessage());
        }
    }

    protected async Task HandleSubmit(EditContext editContext)
    {
        if (_client == null)
            return;

        if (editContext.IsModified())
        {
            if (editContext.Validate())
            {
                ContactNumberDto updatedAddress = (ContactNumberDto)editContext.Model;
                bool isJsonEqual = updatedAddress.JsonCompare(_originalContact);

                if (isJsonEqual) // no changes
                {
                    Message = "No changes found!";
                    _alertNotification.Show(2, true, Message);
                }
                else
                {
                    // changes
                    _logger.Information("Updating contact.");
                    HttpResponseMessage response = await _client.PutAsJsonAsync("api/contactnumber/update", updatedAddress);

                    Message = response.StatusCode.ToString();
                    _logger.Warning("Updated contact {success}.", response.StatusCode);
                    if (response.IsSuccessStatusCode)
                    {
                        _notification.Show(1, true, "You have successfully updated the address!");
                    }
                }
            }
        }
        else
        {
            Message = "No changes found!";
            _alertNotification.Show(2, true, Message);
        }
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
}
