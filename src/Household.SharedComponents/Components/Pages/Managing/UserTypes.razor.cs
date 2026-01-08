using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Pages.Managing;

public partial class UserTypes
{
    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private Loading _loadingIndicator = default!;

    private List<UserType>? _allUserTypes = default!;
    private UserIpDto _userIp = default!;
    private HttpClient? _client;
    private ILogger _logger = default!;

    public string _noneToday = string.Empty;
    private bool _enabled = true;
    private string _visible = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _logger = Logger.ForContext<UserType>();
        _logger.Information("Initializing usertypes");

        _client = ApiService.HttpClient;

        _userIp = await GetUserIpDetails();
        _visible = _userIp.Visible;
        //_disable = _userIp.DisableButton;

        await _loadingIndicator.ShowAsync();
        await GetUserTypesAsync();

        PageHistoryState.AddPageToHistory("/managing/usertypes");
        _enabled = false;
        await _loadingIndicator.HideAsync();
    }

    private async Task GetUserTypesAsync()
    {
        if (_client == null)
        {
            _noneToday = "Cannot make call to retrieve data!";
            _logger.Error("{_}", _noneToday);
            return;
        }

        try
        {
            HttpResponseMessage response = await _client.GetAsync("api/usertype");

            if(response.IsSuccessStatusCode)
            {
                await using Stream stream = await response.Content.ReadAsStreamAsync(new());
                Result<List<UserType>> result = await JsonDeserializer.TryDeserializeAsync<List<UserType>>(stream, new());

                if (!result.IsSuccess)
                {
                    _noneToday = $"Failed to desrialize: {result.Error}";
                    _logger.Error("{_}", _noneToday);
                    return;
                }

                List<UserType>? userList = result.Value;

                if (userList != null)
                    _allUserTypes = userList;
                else
                    _noneToday = "No user type found...";

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.GetInnerMessage());
            throw;
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
