using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Shared;

public partial class AddressbookEntry : ComponentBase
{
    [Parameter]
    public List<AddressInfoDto>? AllAddresses { get; set; } = default!;

    [Parameter]
    public string Letter { get; set; } = string.Empty;

    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private string _noAddressbookEntriesFound = "No addressbook entries for letter ";
    private string _disable = default!;

    private UserIpDto _userIp = default!;
    private HttpClient? _client;
    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<AddressbookEntry>();
        _logger.Information("Initializing addressbook list.");
        _client = ApiService.HttpClient;

        if (!Letter.IsNullOrWhiteSpace())
        {
            _userIp = await GetUserIpDetails();

            _disable = _userIp.DisableButton;
            _logger.Information("{msg}", _userIp.LogMessage);

            await GetMessageForSearch();
        }
    }

    private async Task GetMessageForSearch()
    {
        if (_client == null)
        {
            _noAddressbookEntriesFound = "Cannot make client calls for data!";
            return;
        }

        try
        {
            _logger.Information("{ip} is retrieving the addressbook list to view.", _userIp.IpAddress);
            HttpResponseMessage response = await _client.GetAsync($"api/addressbook/{Letter}");

            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync(new());
            Result<List<AddressInfoDto>> result = await JsonDeserializer.TryDeserializeAsync<List<AddressInfoDto>>(stream, new());

            if (!result.IsSuccess)
            {
                _noAddressbookEntriesFound = $"Failed to deserialize: {result.Error}";
                _logger.Warning("{noEntriesFound}", _noAddressbookEntriesFound);
                return;
            }

            List<AddressInfoDto>? addressInfos = result.Value;

            _logger.Information("{ip} retrieved {count} adressbook entries.", _userIp.IpAddress, addressInfos?.Count);
            if (addressInfos != null && addressInfos.Count > 0)
                AllAddresses = addressInfos;
            else
            {
                AllAddresses = [];
                _noAddressbookEntriesFound = $"No addressbook entries for letter {Letter}";
            }

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
}
