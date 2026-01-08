using System.Net.Http.Headers;
using System.Text;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Pages.DailyShows;

public partial class Index : ComponentBase
{
    [Inject] private IPageHistoryState PageHistory { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;
    [Inject] public ICacheService<List<TVShowDto>> Cache { get; set; } = default!;

    private Loading _loadingIndicator = default!;

    private List<TVShowInformationDto>? _activeShowInformation;
    private List<TVShowDto>? _activeDailyShows;
    private List<TVShowDto>? _inactiveDailyShows;
    private List<TVShowDto>? _overShows;

    private ICacheService<List<TVShowDto>> _cache = default!;
    private const string ACCESS_TOKEN_KEY = CacheKeys.DailyShowsKey;
    //private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly DayOfWeek _dayName = DateTime.Now.DayOfWeek;
    private string _noneToday = string.Empty;

    private bool _enabled = true;
    private string _disable = string.Empty;
    private string _visible = string.Empty;
    private string _selectedTab = "ShowsToWatch";
    private bool _showInputs = true;

    private UserIpDto _userIp = default!;
    private List<TVShowDto>? _allShows = [];
    private HttpClient? _client;
    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _cache = Cache;

        _logger = Logger.ForContext<Index>();
        _logger.Information("Initializing daily shows index");
        _client = ApiService.HttpClient;

        _userIp = await GetUserIpDetails();
        _visible = _userIp.Visible;
        _disable = _userIp.DisableButton;

        _logger.Information("{msg}", _userIp.LogMessage);

        await _loadingIndicator.ShowAsync();
        _showInputs = _loadingIndicator.IsVisible;
        await GetShowInformationAsync();

        _enabled = false;

        PageHistory.AddPageToHistory("/dailyshows/index");
        await _loadingIndicator.HideAsync();
        _showInputs = _loadingIndicator.IsVisible;
        _logger.Information("Daily shows index initialized");
    }

    private Task OnSelectedTabHandler(string nameTab)
    {
        // CRUCIAL: Manually set the selected tab in the C# code
        _selectedTab = nameTab;

        // Add your additional logic here (e.g., loading data for the new tab)
        _logger.Debug($"Tab changed to: {nameTab}");

        return Task.CompletedTask;
    }

    private async Task GetShowInformationAsync()
    {
        _allShows = await _cache.GetOrCreateAsync(ACCESS_TOKEN_KEY, GetAllShowsAsync, TimeSpan.FromHours(3));

        if (_allShows is null)
        {
            _allShows = await GetAllShowsAsync();
            _logger.Information("Cache miss: Retrieved {count} shows from source.", _allShows?.Count);
        }
        else
            _logger.Information("Retrieved {count} shows from cache.", _allShows?.Count);

        await GetDailyShowsAsync();

        if (_activeDailyShows is not null)
        {
            string content = JsonConvert.SerializeObject(_activeDailyShows?.Select(tv => tv.Name).ToList());
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            ByteArrayContent byteContent = new(buffer);

            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            CancellationTokenSource cts = new();
            CancellationToken cancellationToken = cts.Token;

            try
            {
                HttpRequestMessage request = new()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_client?.BaseAddress + "api/tvshowinformation/showinformation"),
                    Content = byteContent,
                };

                using HttpResponseMessage? response = await _client?.SendAsync(request, cancellationToken)!;
                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<List<TVShowInformationDto>?> result = await JsonDeserializer.TryDeserializeAsync<List<TVShowInformationDto>?>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    _noneToday = $"Failed to deserialize: {result.Error}";
                    _logger.Error("{msg}", _noneToday);
                    return;
                }

                _activeShowInformation = result.Value;
            }
            catch (OperationCanceledException)
            {
                _logger.Warning("Get show information operation was cancelled by user.");
            }
            catch (HttpRequestException hrex)
            {
                _logger.Error(hrex, "{msg}", hrex.GetInnerMessage());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Encountered an Error in GetShowInformationAsync: {errMsg}", ex.GetInnerMessage());
            }
            finally
            {
                cts.Dispose();
            }
        }
    }

    private async Task GetDailyShowsAsync()
    {
        try
        {
            _logger.Warning("{ip} retrieved {count} shows for {dayOfWeek}", _userIp.IpAddress, _allShows?.Count, _dayName);

            if (_allShows != null && _allShows.Count > 0)
            {
                _activeDailyShows = [.. _allShows.Where(s => s.IsCompleted == false && s.IsCompletedSeason == false)];
                _inactiveDailyShows = [.. _allShows.Where(s => s.IsCompleted == false && s.IsCompletedSeason == true)];
                _overShows = [.. _allShows.Where(s => s.IsCompleted == true)];
            }
            else
                _noneToday = "No shows for today...";

        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered an error in GetDailyShowsAsync: {errMsg}", ex.GetInnerMessage());
        }
    }
    
    private async Task<List<TVShowDto>> GetAllShowsAsync()
    {
        if (_client == null)
        {
            _noneToday = "Cannot make client calls for data!";
            _logger.Warning("{msg}", _noneToday);
            return new();
        }

        CancellationTokenSource cts = new();
        CancellationToken cancellationToken = cts.Token;

        try
        {
            _logger.Warning("{ip} is retrieving all shows for today {weekDay}", _userIp.IpAddress, _dayName);
            HttpResponseMessage responseMessage = await _client.GetAsync($"api/shows/dayofweek/{(int)_dayName}", cancellationToken);

            responseMessage.EnsureSuccessStatusCode();
            await using Stream stream = await responseMessage.Content.ReadAsStreamAsync(new());
            Result<List<TVShowDto>> result = await JsonDeserializer.TryDeserializeAsync<List<TVShowDto>>(stream, new());

            if (!result.IsSuccess)
            {
                _noneToday = $"Failed to deserialize: {result.Error}";
                _logger.Error("{msg}", _noneToday);
                return new();
            }

            List<TVShowDto> tvShows = result?.Value!;

            _logger.Information("{ip} retrieved {count} shows for {dayOfWeek}", _userIp.IpAddress, tvShows?.Count, _dayName);

            if (tvShows != null && tvShows.Count > 0)
            {
                _allShows = tvShows;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Warning("Get daily shows operation was cancelled by user.");
        }
        catch (HttpRequestException hrex)
        {
            _logger.Error(hrex, "{msg}", hrex.GetInnerMessage());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered an error in GetDailyShowsAsync: {errMsg}", ex.GetInnerMessage());
        }
        finally
        {
            cts.Dispose();
        }
        
        return _allShows ?? [];
    }

    private async Task<UserIpDto> GetUserIpDetails()
    {
        UserIpDto userIp = new();
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
