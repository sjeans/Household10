using System.Net.Http.Json;
using Household.Shared.Dtos;
using Household.Shared.Enums;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;
using Household.SharedComponents.Components.Shared;
using Household.SharedComponents.Components.Shared.Loader;

namespace Household.SharedComponents.Components.Pages.Managing.ShowInformation;

public partial class AddShowDetail
{
    [Inject] public NavigationManager NavManager { get; set; } = default!;
    [Inject] public required IApiService ApiService { get; set; }
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;
    //[Inject] private IUserIpService UserIpService { get; set; } = default!;
    [Inject] public ICacheService<List<TVShowDto>> Cache { get; set; } = default!;

    private ICacheService<List<TVShowDto>> _cache = default!;
    private const string ACCESS_TOKEN_KEY = CacheKeys.DailyShowsKey;

    private Loading _loadingIndicator = default!;

    private SuccessNotification _notification = new();

    private string _searchTerm = string.Empty;

    private TvShowInformation? _showInformation = default!;
    private MarkupString? _showSummary;
    private MarkupString? episodeSummary;
    private string _showName = string.Empty;
    private string _showImgSrc = string.Empty;
    private string? _showEnded;
    private string _episodeName = string.Empty;
    private string _episodeImgSrc = string.Empty;
    private string _episodeAirTime = string.Empty;

    private HttpClient? _client;
    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _logger = Logger.ForContext<AddShowDetail>();
        _logger.Information("Initializing find show details.");
        _client = ApiService.HttpClient;
        //UserIpService.GetUserIP();
        _cache = Cache;
    }

    private async Task<int> GetSubscriptionIdAsync(string streamingName)
    {
        if (_client != null)
        {
            HttpResponseMessage response = await _client.GetAsync("api/subscriptions");

            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync(new());
            Result<List<StreamingService>> result = await JsonDeserializer.TryDeserializeAsync<List<StreamingService>>(stream, new());

            if(!result.IsSuccess)
            {
                _logger.Error("Failed to deserialize: {msg}", result.Error);
                return -1;
            }

            List<StreamingService>? services = result.Value;

            if (services != null)
            {
                string subscriptionName = streamingName switch
                {
                    "HBO" => "Max",
                    "HBO Max" => "Max",
                    "CBS" => "Paramount+",
                    "Now" => "Peacock",
                    "NBC" => "Peacock",
                    "ABC" => "Hulu",
                    "AMC" => "Amazon",
                    "AMC+" => "Amazon",
                    "SVT Play" => "Amazon",
                    "Prime Video" => "Amazon",
                    "Apple TV+" => "Apple+",
                    "Apple TV" => "Apple+",
                    _ => streamingName,
                };

                return services.FirstOrDefault(s => s.Name.Equals(subscriptionName))!.Id;
            }
        }

        return 0;
    }

    protected async Task SearchIssuesAsync()
    {
        if (_client == null)
        {
            _logger.Error("Cannot make client calls to retrieve data!");
            return;
        }

        if (!_searchTerm.IsNullOrWhiteSpace())
        {
            await _loadingIndicator.ShowAsync();

            _logger.Information("Finding a new show."/*, UserIpService.IpAddress*/);
            HttpResponseMessage response = await _client.GetAsync($"api/tvshowinformation/retrieveshowinformation/{_searchTerm}/{false}");

            _logger.Information("Update success: {success}.", /*UserIpService.IpAddress,*/ response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                await using Stream stream = response.Content.ReadAsStream(new());
                Result<TvShowInformation> result = await JsonDeserializer.TryDeserializeAsync<TvShowInformation>(stream, new());

                if (!result.IsSuccess)
                {
                    _logger.Error("Failed to deserialize: {msg}", result.Error);
                    return;
                }

                TvShowInformation foundShow = result.Value!;

                if (foundShow is not null)
                {
                    string time = foundShow?.Schedule?.Time ?? string.Empty;
                    _episodeAirTime = !time.IsNullOrWhiteSpace() ? DateTime.Parse(time).ToString(@"h\:mm\:ss tt") : string.Empty;
                    _showName = foundShow!.Name;
                    _showSummary = new(foundShow!.Summary ?? string.Empty);
                    _showImgSrc = foundShow.Images?.Medium ?? string.Empty;
                    _showEnded = foundShow.Ended.IsNullOrWhiteSpace() ? null : Convert.ToDateTime(foundShow!.Ended).ToShortDateString();
                    _showInformation = foundShow;
                }
            }

            await _loadingIndicator.HideAsync();
        }
    }

    protected void EpisodeUpdate(int episodeDetailId)
    {
        if (episodeDetailId > 0)
        {
            Episode? episode = _showInformation?.Episodes?.FirstOrDefault(ep => ep.TvMazeId == episodeDetailId);
            if (episode != null)
            {
                _episodeImgSrc = episode.Images?.Medium ?? string.Empty;
                _episodeName = episode.Name;
                episodeSummary = new(episode?.Summary ?? string.Empty);
            }
        }
    }

    protected async Task HandleSubmit()
    {
        if (_showInformation is null)
        {
            _logger.Error("Detail information cannot be null!");
            return;
        }

        if (_client == null)
        {
            _logger.Error("Cannot make client calls for data!");
            return;
        }

        await _loadingIndicator.ShowAsync();
        HttpResponseMessage response = await _client.GetAsync("api/tvshowinformation/names");

        response.EnsureSuccessStatusCode();

        await using Stream stream = await response.Content.ReadAsStreamAsync(new());
        Result<Dictionary<int, string>> result = await JsonDeserializer.TryDeserializeAsync<Dictionary<int, string>>(stream, new());

        if (!result.IsSuccess)
        {
            _logger.Error("Failed to deserialize: {msg}", result.Error);
            return;
        }

        Dictionary<int, string>? showNames = result.Value;

        if (showNames is not null)
        {
            DayOfWeek dayOfWeek = _showInformation.Schedule?.Days?.FirstOrDefault() switch
            {
                "Sunday" => DayOfWeek.Sunday,
                "Monday" => DayOfWeek.Monday,
                "Tuesday" => DayOfWeek.Tuesday,
                "Wednesday" => DayOfWeek.Wednesday,
                "Thursday" => DayOfWeek.Thursday,
                "Friday" => DayOfWeek.Friday,
                "Saturday" => DayOfWeek.Saturday,
                _ => 0
            };

            Network? network = (!_showInformation.Network.IsNullOrWhiteSpace()) ? JsonConvert.DeserializeObject<Network>(_showInformation.Network) : null;
            string name = _showInformation?.WebChannel != null ? _showInformation?.WebChannel?.Name! : network?.Name!;

            List<Episode>? episodes = _showInformation?.Episodes;
            _ = int.TryParse(episodes?.LastOrDefault()?.Season!.Value.ToString(), out int season);
            _ = int.TryParse(episodes?.LastOrDefault()?.Number!.Value.ToString(), out int number);
            TVShowDto tvShow = new()
            {
                DayOfWeek = dayOfWeek,
                Season = (Seasons)season,
                Episodes = number,
                Name = _showInformation?.Name ?? string.Empty,
                Rating = _showInformation?.Rating?.Average == null ? 0 : Convert.ToDecimal(_showInformation?.Rating?.Average!.Value),
                Time = episodes?.LastOrDefault()?.AirStamp!.Value.ToShortTimeString() ?? string.Empty,
                StartDate = episodes?.FirstOrDefault(ep => ep.Season == season && ep.Number == 1)?.AirDate ?? string.Empty,
                StreamingId = await GetSubscriptionIdAsync(name),
            };

            //_logger.Information("{ip} is adding new show.", UserIpService.IpAddress);
            response = await _client.PostAsJsonAsync("api/shows/", tvShow);
            //_logger.Information("Add from {ip} was success: {success}", UserIpService.IpAddress, tvshowResponse.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                _notification?.Show("You have successfully updated the show!");
                //actionType = "updated";
            }

            // Now expire the cache for the shows list and create a new one
            // Key: HouseholdCache_Household:DailyShows
            _logger.Information("Expired cache {success}", await _cache.ExpireAsync(ACCESS_TOKEN_KEY));
        }

        await _loadingIndicator.HideAsync();
    }
}
