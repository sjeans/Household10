using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Pages.AddressBook;

public partial class Index
{
    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private string _noneToday = string.Empty;
    private List<AddressInfoDto> _allAddresses = [];
    private bool _enabled = true;

    private Loading _loadingIndicator = default!;
    private bool _showInputs;

    private UserIpDto _userIp = default!;
    private string _disable = default!;
    private string _visible = default!;
    private HttpClient? _client;
    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<Index>();
        _logger.Information("Initializing addressbook list.");
        _client = ApiService.HttpClient;
        if (_allAddresses.Count == 0)
        {
            _userIp = await GetUserIpDetails();
            await _loadingIndicator.ShowAsync();
            _showInputs = _loadingIndicator.IsVisible;
            await GetAllAddressesAsync();

            _visible = _userIp.Visible;
            _disable = _userIp.DisableButton;
            _logger.Information("{msg}", _userIp.LogMessage);
            PageHistoryState.AddPageToHistory("addressbook");
            _enabled = false;
            await _loadingIndicator.HideAsync();
            _showInputs = _loadingIndicator.IsVisible;
        }
    }

    private async Task GetAllAddressesAsync()
    {
        if (_client == null)
        {
            _noneToday = "Cannot make client calls for data!";
            return;
        }

        try
        {
            _logger.Information("{ip} is retrieving the addressbook list to view.", _userIp.IpAddress);
            HttpResponseMessage responseMessage = await _client.GetAsync("api/addressbook");

            responseMessage.EnsureSuccessStatusCode();

            await using Stream stream = responseMessage.Content.ReadAsStream(new());
            Result<List<AddressInfoDto>> result = await JsonDeserializer.TryDeserializeAsync<List<AddressInfoDto>>(stream, new());

            if (!result.IsSuccess)
            {
                _noneToday = $"Failed to deserialize: {result.Error}";
                _logger.Warning("{msg}", _noneToday);
                return;
            }

            List<AddressInfoDto>? addressInfos = result.Value;

            _logger.Information("{ip} retrieved {count} adressbook entries.", _userIp.IpAddress, addressInfos?.Count);
            if (addressInfos != null && addressInfos.Count > 0)
                _allAddresses = addressInfos;
            else
                _noneToday = "No Addresses found";

        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered an error retrieving addressbook list! Error: {errMsg}", ex.GetInnerMessage());
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

    //private async Task GetIdentifiers()
    //{
    //    identifiers = await JS.InvokeAsync<List<string>>("findLocalIdentifiers");
    //    //af54baa1-f148-41b3-9b65-2adb0bdc24d3.local

    //    string ip = "192.168.1.42"; // Example
    //    try
    //    {
    //        IPHostEntry hostEntry = Dns.GetHostEntry(IPAddress.Parse(ip));
    //        Console.WriteLine($"Hostname: {hostEntry.HostName}");
    //    }
    //    catch (SocketException ex)
    //    {
    //        Console.WriteLine($"Reverse lookup failed: {ex.Message}");
    //    }
    //}
}
