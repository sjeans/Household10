using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Pages.Managing;

public partial class Users
{
    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private Loading _loadingIndicator = default!;

    private UserIpDto _userIp = default!;
    private HttpClient _client = default!;
    private ILogger _logger = default!;
    private List<User>? _allUsers = default!;
    private bool _enabled = true;
    //private string _disable = string.Empty;
    private string _visible = string.Empty;
    private string _noneToday = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<Users>();
        _logger.Information("Initializing add show.");
        
        _client = ApiService.HttpClient;
        _userIp = await GetUserIpDetailsAsync();
        _visible = _userIp.Visible;

        _logger.Information("{msg}", _userIp.LogMessage);

        await _loadingIndicator.ShowAsync();
        await GetUsersAsync();
        _enabled = false;

        await _loadingIndicator.HideAsync();
    }

    private async Task GetUsersAsync()
    {
        try
        {
            if (_client == null)
            {
                _noneToday = "Cannot make client call to retreive data!";
                _logger.Error("{_}", _noneToday);
                return;
            }

            _logger.Information("Retrieving all users");
            HttpResponseMessage response = await _client.GetAsync("api/user");

            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync(new());
            Result<List<User>> result = await JsonDeserializer.TryDeserializeAsync<List<User>>(stream, new());

            if (!result.IsSuccess)
            {
                _noneToday = $"Failed to desrialize: {result.Error}";
                _logger.Error("{_}", _noneToday);
                return;
            }

            List<User>? userList = result.Value;
            _logger.Information("{count} retrieved users", userList?.Count);

            if (userList != null)
                _allUsers = userList;
            else
                _noneToday = "No users found today...";

            PageHistoryState.AddPageToHistory("/managing/users");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error encountered retrieving all users: {errMsg}", ex.GetInnerMessage());
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
}
