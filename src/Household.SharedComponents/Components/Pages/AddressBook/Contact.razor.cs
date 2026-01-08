using Household.Shared.Dtos;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Pages.AddressBook;

public partial class Contact
{
    [Parameter]
    public string Letter { get; set; } = string.Empty;

    [Inject] public IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] public required IApiService ApiService { get; set; }
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private List<AddressInfoDto> _allAddresses = default!;
    //private bool _enabled;

    private Loading _loadingIndicator = default!;

    private UserIpDto _userIp = default!;
    private string _disable = default!;
    private string _visible = default!;
    private HttpClient? _client;
    private ILogger _logger = default!;

    protected override async void OnInitialized()
    {
        _logger = Logger.ForContext<Contact>();
        _logger.Information("Initializing contact.");
        _client = ApiService.HttpClient;
        _userIp = await GetUserIpDetails();

        _visible = _userIp.Visible;
        _disable = _userIp.DisableButton;
        await _loadingIndicator.ShowAsync();
        GetPageHistory();
        await _loadingIndicator.HideAsync();
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

    private void GetPageHistory()
    {
        PageHistoryState.AddPageToHistory("addressbook/index");
    }
}
