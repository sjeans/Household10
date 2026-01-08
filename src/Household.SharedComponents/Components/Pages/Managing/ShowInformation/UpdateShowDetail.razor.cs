using System.Net.Http.Headers;
using System.Text;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;
using Household.SharedComponents.Components.Shared;
using Household.SharedComponents.Components.Shared.Loader;

namespace Household.SharedComponents.Components.Pages.Managing.ShowInformation;

public partial class UpdateShowDetail : ComponentBase
{
    [Inject] public required IApiService ApiService { get; set; }
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private Loading _loadingIndicator = default!;

    private SuccessNotification _notification = new();

    private int _showDetailId;
    private string? _showImgSrc = string.Empty;
    private MarkupString? _showSummary;
    private string? _showName = string.Empty;
    private string? _episodeAirTime = string.Empty;
    private bool _moreEpisodes;
    private bool _needsUpdated;
    private MarkupString? _whichNeedsUpdated;
    private MarkupString? _whichNeedsAdded;
    private Dictionary<string, int> _seasonEpisodeToUpdate = default!;
    private Dictionary<string, int> _seasonEpisodeToAdd = default!;

    private UserIpDto _userIp = default!;
    private HttpClient? _client;
    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _logger = Logger.ForContext<UpdateShowDetail>();
        _logger.Information("Initializing find show details.");
        _client = ApiService.HttpClient;
        _userIp = await GetUserIpDetails();
    }

    protected async Task UpdateShowInformationAsync(int showDetailId)
    {
        _showDetailId = showDetailId;
        if (_client != null)
        {
            await _loadingIndicator.ShowAsync();
            HttpResponseMessage response = await _client.GetAsync("api/tvshowinformation/names");

            response.EnsureSuccessStatusCode();

            await using Stream stream = response.Content.ReadAsStream(new());
            Result<Dictionary<int, string>> result = await JsonDeserializer.TryDeserializeAsync<Dictionary<int, string>>(stream, new());

            if (!result.IsSuccess)
            {
                _logger.Error("Failed to deserialize: {msg}", result.Error);
                return;
            }

            Dictionary<int, string>? showNames = result.Value;

            if (showNames is not null)
            {
                TVShowInformationDto? localShowInformation = new();
                string? showDetailName = showNames.FirstOrDefault(x => x.Key == _showDetailId).Value;

                string myContent = JsonConvert.SerializeObject(new List<string>() { showDetailName });
                byte[] buffer = Encoding.UTF8.GetBytes(myContent);
                ByteArrayContent byteContent = new(buffer);

                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                response = await _client.PostAsync("api/tvshowinformation/showinformation", byteContent);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        await using Stream stream2 = response.Content.ReadAsStream(new());
                        Result<List<TVShowInformationDto>> result2 = await JsonDeserializer.TryDeserializeAsync<List<TVShowInformationDto>>(stream2, new());

                        if (!result2.IsSuccess)
                        {
                            _logger.Error("Failed to deserialize: {msg}", result2.Error);
                            return;
                        }

                        List<TVShowInformationDto>? showDetails = result2.Value;
                        localShowInformation = showDetails?.FirstOrDefault();
                        _showName = localShowInformation?.Name;
                        _showImgSrc = localShowInformation?.Image?.Medium;
                        _showSummary = new(localShowInformation?.Summary ?? string.Empty);
                        _showDetailId = localShowInformation!.Id;
                        _episodeAirTime = localShowInformation?.Episodes?.FirstOrDefault()?.AirTime;
                    }
                    catch(Exception ex)
                    {
                        _logger.Error(ex, "{msg}", ex.GetInnerMessage());
                    }
                }

                response = await _client.GetAsync($"api/episodedetails/liveepisodes/{localShowInformation?.TvMazeId}");

                response.EnsureSuccessStatusCode();

                await using Stream stream1 = await response.Content.ReadAsStreamAsync(new());
                Result<List<Episode>> result1 = await JsonDeserializer.TryDeserializeAsync<List<Episode>>(stream1, new());

                if (!result1.IsSuccess)
                {
                    _logger.Error("Failed to deserialize: {msg}", result1.Error);
                    return;
                }

                List<Episode>? liveEpisodes = result1.Value;

                if (liveEpisodes != null)
                {
                    try
                    {
                        List<Episode>? localEpisodes = localShowInformation?.Episodes;
                        List<string> whichNeedsUpdated = [];
                        List<string> whichNeedsAdded;

                        _whichNeedsUpdated = new();
                        _whichNeedsAdded = new();
                        _seasonEpisodeToUpdate = [];
                        _seasonEpisodeToAdd = [];
                        _needsUpdated = false;
                        List<int> episodeIds = [];

                        _moreEpisodes = CheckAndSetMoreEpisodes(liveEpisodes, localEpisodes!, out whichNeedsAdded);

                        foreach (Episode? episode in liveEpisodes)
                        {
                            Episode? local = localEpisodes?.FirstOrDefault(x => x.Season == episode.Season && x.Number == episode.Number);

                            //var a = local.Summary == episode.Summary;
                            //var b = local.AirStamp == episode?.AirStamp;
                            //var c = local.AirDate == episode?.AirDate;
                            //var d = local.AirTime == episode?.AirTime;
                            //var e = local.Images?.Medium == episode?.Images?.Medium;
                            //var f = local.Images?.Original == episode?.Images?.Original;
                            //var g = local.Links?.PreviousEpisode?.Href == episode?.Links?.PreviousEpisode?.Href;
                            //var h = local.Links?.Self?.Href == episode?.Links?.Self?.Href;
                            //var i = local.Links?.Show?.Href == episode?.Links?.Show?.Href;
                            //var j = local.Rating?.Average == episode?.Rating?.Average;

                            //int numberOfChanges = PropertiesToCompare.Count(func => !func(episode, local!));

                            Dictionary<string, bool> differences = PropertiesToCompare
                                .Select(func => func(episode, local!)) // Execute each comparison function
                                .SelectMany(dict => dict) // Flatten the array of dictionaries into key-value pairs
                                .Where(kvp => kvp.Value) // Keep only differences (where the boolean is true)
                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value); // Convert back to a dictionary

                            if(differences.Any())
                            //if (numberOfChanges > 0)
                            {
                                _needsUpdated = true;
                                _ = int.TryParse(episode?.Season.ToString(), out int season);
                                _ = int.TryParse(episode?.Number.ToString(), out int number);
                                whichNeedsUpdated.Add($"{season}: <a href=\"{episode?.Url}\" target=_blank>{number}</a>");
                                episodeIds.Add(season);
                                episodeIds.Add(number);

                                _seasonEpisodeToUpdate.Add(string.Join(",", episodeIds), season);

                                episodeIds = [];
                            }
                        }

                        if (whichNeedsUpdated.Count > 0)
                            _whichNeedsUpdated = new(string.Join(",<br /> ", whichNeedsUpdated));

                        if (whichNeedsAdded.Count > 0)
                            _whichNeedsAdded = new(string.Join(",<br /> ", whichNeedsAdded));

                    }
                    catch(Exception ex)
                    {
                        _logger.Error(ex, "{msg}", ex.GetInnerMessage());
                    }
                }

            }
            
            await _loadingIndicator.HideAsync();
        }
    }

    //private readonly Func<Episode, Episode, bool>[] PropertiesToCompare =
    //    [
    //        (le, ee) => le.Summary == ee?.Summary,
    //        (le, ee) => le.AirStamp == ee?.AirStamp,
    //        (le, ee) => le.AirDate == ee?.AirDate,
    //        (le, ee) => le.AirTime == ee?.AirTime,
    //        (le, ee) => le.Images?.Medium == ee?.Images?.Medium,
    //        (le, ee) => le.Images?.Original == ee?.Images?.Original,
    //        (le, ee) => le.Links?.PreviousEpisode?.Href == ee?.Links?.PreviousEpisode?.Href,
    //        (le, ee) => le.Links?.Self?.Href == ee?.Links?.Self?.Href,
    //        (le, ee) => le.Links?.Show?.Href == ee?.Links?.Show?.Href,
    //        (le, ee) => le.Rating?.Average == ee?.Rating?.Average
    //    ];

    private readonly Func<Episode, Episode, Dictionary<string, bool>>[] PropertiesToCompare =
   [
        (le, ee) => new Dictionary<string, bool> { { "Summary", le.Summary != ee?.Summary } },
        (le, ee) => new Dictionary<string, bool> { { "AirStamp", le.AirStamp.HasValue && ee?.AirStamp.HasValue == true ? le.AirStamp.Value.ToUniversalTime() != ee.AirStamp.Value : le.AirStamp.HasValue != ee?.AirStamp.HasValue } },
        (le, ee) => new Dictionary<string, bool> { { "AirDate", le.AirDate != ee?.AirDate } },
        (le, ee) => new Dictionary<string, bool> { { "AirTime", le.AirTime != ee?.AirTime } },
        (le, ee) => new Dictionary<string, bool> { { "Images.Medium", le.Images?.Medium != ee?.Images?.Medium } },
        (le, ee) => new Dictionary<string, bool> { { "Images.Original", le.Images?.Original != ee?.Images?.Original } },
        (le, ee) => new Dictionary<string, bool> { { "Links.PreviousEpisode.Href", le.Links?.PreviousEpisode?.Href != ee?.Links?.PreviousEpisode?.Href } },
        (le, ee) => new Dictionary<string, bool> { { "Links.Self.Href", le.Links?.Self?.Href != ee?.Links?.Self?.Href } },
        (le, ee) => new Dictionary<string, bool> { { "Links.Show.Href", le.Links?.Show?.Href != ee?.Links?.Show?.Href } },
        (le, ee) => new Dictionary<string, bool> { { "Rating.Average", le.Rating?.Average != ee?.Rating?.Average } }
    ];

    private bool CheckAndSetMoreEpisodes(List<Episode> liveEpisodes, List<Episode> localEpisodes, out List<string> whichNeedsAdded)
    {
        whichNeedsAdded = [];

        if (liveEpisodes == null || localEpisodes == null)
            throw new ArgumentNullException("One or both of the episode lists are null.");

        if (liveEpisodes.Count > localEpisodes.Count)
        {
            List<int> episodeIds = [];

            for (int i = localEpisodes.Count; i < liveEpisodes.Count; i++) {
                episodeIds.Add(liveEpisodes[i].Season!.Value);
                episodeIds.Add(liveEpisodes[i].Number!.Value);
                whichNeedsAdded.Add($"{liveEpisodes[i].Season!.Value}: <a href=\"{liveEpisodes[i]?.Url}\" target=_blank>{liveEpisodes[i].Number!.Value}</a>");
                _seasonEpisodeToAdd.Add(string.Join(",", episodeIds), liveEpisodes[i].Season!.Value);
                episodeIds = [];
            }

            return true;
        }
        else
            return false;

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

    protected async Task HandleUpdateAll()
    {
        if (_showDetailId == 0)
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

        if (_seasonEpisodeToUpdate.Count > 0)
        {
            string myContent = JsonConvert.SerializeObject(_seasonEpisodeToUpdate);
            byte[] buffer = Encoding.UTF8.GetBytes(myContent);
            ByteArrayContent byteContent = new(buffer);

            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await _client.PutAsync($"api/tvshowinformation/UpdateShowEpisode/{_showDetailId}", byteContent);

            if(response.IsSuccessStatusCode)
            {
                _notification?.Show("You have successfully updated the show!");
                _notification?.Navigation?.Refresh(true);
            }
        }

        await _loadingIndicator.HideAsync();
    }

    protected async Task HandleAddNewEpisodes()
    {
        if (_showDetailId == 0)
        {
            _logger.Error("Detail information cannot be null!");
            return;
        }

        if (_client == null)
        {
            _logger.Error("Cannot make client calls for data!");
            return;
        }

        if(_seasonEpisodeToAdd.Count > 0)
        {
            string myContent = JsonConvert.SerializeObject(_seasonEpisodeToAdd);
            byte[] buffer = Encoding.UTF8.GetBytes(myContent);
            ByteArrayContent byteContent = new(buffer);

            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await _client.PutAsync($"api/tvshowinformation/AddShowEpisode/{_showDetailId}", byteContent);

            if(response.IsSuccessStatusCode)
            {
                _notification?.Show("You have successfully updated the show!");
                _notification?.Navigation?.Refresh(true);
            }
        }
    }
}
