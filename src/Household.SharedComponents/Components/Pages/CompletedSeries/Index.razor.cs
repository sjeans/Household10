using System.Net.Http.Headers;
using System.Text;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Pages.CompletedSeries;

public partial class Index : ComponentBase
{
    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] ILogger Logger { get; set; } = default!;
    [Inject] IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private List<TVShowDto>? _inactiveDailyShows = default!;
    private List<TVShowDto>? _overShows = default!;

    private Loading _loadingIndicator = default!;
    private bool _showInputs = true;

    private bool _enabled = true;
    private string _disable = string.Empty;
    private string _visible = string.Empty;

    private List<TVShowDto>? _allShows = [];
    private List<TVShowInformationDto>? _activeShowInformation = default!;
    private string? _noneFound;

    private UserIpDto _userIp = new();
    private string _selectedTab = "SeasonOver";
    private HttpClient? _client;
    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<Index>();
        _logger.Warning("Initializing daily shows index");
        _client = ApiService.HttpClient;

        PageHistoryState.AddPageToHistory("/dailyshows/");

        _userIp = await GetUserIpDetails();
        _visible = _userIp.Visible;
        _disable = _userIp.DisableButton;
        _logger.Information("{msg}", _userIp.LogMessage);

        await _loadingIndicator.ShowAsync();
        _showInputs = _loadingIndicator.IsVisible;
        await GetCompletedSeriesAsync();
        await GetShowInformationAsync();
        await _loadingIndicator.HideAsync();
        _showInputs = _loadingIndicator.IsVisible;
    }

    private async Task GetCompletedSeriesAsync()
    {
        if (_client == null)
        {
            _noneFound = "Cannot make client calls for data!";
            _logger.Warning(_noneFound);
            return;
        }

        try
        {
            _logger.Information("{ip} is retrieving all completed shows", _userIp.IpAddress);
            HttpResponseMessage response = await _client.GetAsync("api/shows/completedShows");

            response.EnsureSuccessStatusCode();

            await using Stream stream = response.Content.ReadAsStream(new());
            Result<List<TVShowDto>> result = await JsonDeserializer.TryDeserializeAsync<List<TVShowDto>>(stream, new());

            if(!result.IsSuccess)
            {
                _noneFound = $"Failed to deserialize: {result.Error}";
                _logger.Warning(_noneFound);
                return;
            }

            List<TVShowDto>? tvShows = result.Value;
            _logger.Information("{ip} retrieved {count} completed shows", _userIp.IpAddress, tvShows?.Count);

            if (tvShows != null && tvShows.Count > 0)
            {
                _allShows = tvShows;
                //_activeDailyShows = _allShows.Where(s => s.IsCompleted == false && s.IsCompletedSeason == false).ToList();
                _inactiveDailyShows = _allShows.Where(s => s.IsCompleted == false && s.IsCompletedSeason == true).ToList();
                _overShows = _allShows.Where(s => s.IsCompleted == true).ToList();
            }
            else
                _noneFound = "No shows found...";

        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered error retrieving completed series. Error: {errMsg}", ex.GetInnerMessage());
        }
    }

    private async Task GetShowInformationAsync()
    {
        string content = JsonConvert.SerializeObject(_allShows?.Select(tv => tv.Name).ToList());
        byte[] buffer = Encoding.UTF8.GetBytes(content);
        ByteArrayContent byteContent = new(buffer);

        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        try
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client?.BaseAddress + "api/tvshowinformation/showinformation"),
                Content = byteContent,
            };

            HttpResponseMessage? response = await _client?.SendAsync(request)!;

            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync(new());
            Result<List<TVShowInformationDto>> result = await JsonDeserializer.TryDeserializeAsync<List<TVShowInformationDto>>(stream, new());

            if(!result.IsSuccess)
            {
                _noneFound = $"Failed to deserialize: {result.Error}";
                _logger.Warning(_noneFound);
                return;
            }

            _activeShowInformation = result.Value;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered an Error: {errMsg}", ex.GetInnerMessage());
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

    private Task OnSelectedTabChanged(string name)
    {
        _selectedTab = name;

        return Task.CompletedTask;
    }
}
