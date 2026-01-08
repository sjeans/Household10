using System.Net.Http.Headers;
using System.Text;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Pages.EpisodeDetails;

public partial class Index
{
    [Parameter]
    public int ShowId { get; set; }

    [Parameter]
    public int Season { get; set; }

    [Parameter] 
    public int EpisodeId { get; set; }

    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private string _showName = string.Empty;
    private Episode? _showEpisode = null;
    private MarkupString _summaryDisplay = new();

    Loading _loadingIndicator = default!;

    private HttpClient? _client;
    private ILogger _logger = default!;

    private TvShowInformation? _showInformation = default!;
    private UserIpDto _userIp = default!;

    private int _episodeSeason = 0;
    private int _episodeNumber = 0;
    private string _episodeName = string.Empty;
    private string _episodeImgSrc = string.Empty;
    private string _episodeAirTime = string.Empty;
    private string _episodeRating = string.Empty;
    private string _episodeAirDate = string.Empty;
    private string _episodeRuntime = string.Empty;
    private string _missingImg = string.Empty;
    private string _officialSite = string.Empty;
    private string _streamingName = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<Index>();
        _logger.Information("Initializing episode detail");
        _client = ApiService.HttpClient;

        _userIp = await GetUserIpDetails();
        _logger.Information("{msg}", _userIp.LogMessage);

        await _loadingIndicator.ShowAsync();
        await GetEpisodeDetailAsync();

        PageHistoryState.AddPageToHistory("/dailyshows");
        _logger.Information("Episode detail initialized");
        await _loadingIndicator.HideAsync();
    }

    private async Task GetEpisodeDetailAsync()
    {
        _showName = $"Show Id: {ShowId}, Season: {Season}, Episode Id: {EpisodeId}";

        if (_client is null)
        {
            _logger.Warning("Cannot make client calls for data!");
            return;
        }

        try
        {
            _logger.Information("{ip} is retrieving episode detail for episode id {id}", _userIp.IpAddress, EpisodeId);

            HttpResponseMessage response = await _client.GetAsync($"api/episodedetails/{ShowId}/{Season}/{EpisodeId}");

            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync(new());
            Result<Episode> result = await JsonDeserializer.TryDeserializeAsync<Episode>(stream, new());

            if(!result.IsSuccess)
            {
                _logger.Error("Failed to deserialize: {msg}", result.Error);
                return;
            }

            Episode? episode = result.Value;

            _logger.Information("{ip} retrieved {count} episode detail for {name}", _userIp.IpAddress, episode?.Name, _showName);
            if (episode is not null)
            {
                _showName = episode.TvShowInformation.Name;
                await GetEpisodeListAsync();
                await SetEpisodeInformation(episode);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered an error: {errMsg}", ex.GetInnerMessage());
        }
    }

    private async Task GetEpisodeListAsync()
    {
        if (_client is null)
        {
            _logger.Warning("Cannot make client calls for data!");
            return;
        }

        TVShowInformationDto? localShowInformation = new();
        string content = JsonConvert.SerializeObject(new List<string>() { _showName });
        byte[] buffer = Encoding.UTF8.GetBytes(content);
        ByteArrayContent byteContent = new(buffer);

        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpResponseMessage response = await _client.PostAsync("api/tvshowinformation/showinformation", byteContent);

        response.EnsureSuccessStatusCode();

        await using Stream stream = response.Content.ReadAsStream(new());
        Result<List<TVShowInformationDto>> result = await JsonDeserializer.TryDeserializeAsync<List<TVShowInformationDto>>(stream, new());

        if(!result.IsSuccess)
        {
            _logger.Error("Failed to deserialize: {msg}", result.Error);
            return;
        }

        List<TVShowInformationDto>? showDetails = result.Value;

        try
        {
            localShowInformation = showDetails?.FirstOrDefault();
            _showInformation = new()
            {
                Episodes = localShowInformation?.Episodes?.Where(ep => ep.Season == Season).OrderBy(s => s.Season).ThenBy(n => n.Number).ToList(),
                AverageRuntime = localShowInformation?.AverageRuntime,
                Summary = localShowInformation?.Summary
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "{msg}", ex.GetInnerMessage());
        }
    }

    protected void EpisodeUpdate(int episodeDetailId)
    {
        if (episodeDetailId > 0)
        {
            Episode? episode = _showInformation?.Episodes?.FirstOrDefault(ep => ep.TvMazeId == episodeDetailId);
            if (episode is not null)
            {
                _showName = episode.Name;
                SetEpisodeInformation(episode);
            }
        }
    }

    private Task SetEpisodeInformation(Episode episode)
    {
        _showEpisode = episode;

        DateTime? airStamp = _showEpisode.AirStamp;
        string? airDate = _showEpisode.AirDate;

        _streamingName = _showInformation?.Episodes?.FirstOrDefault()?.TvShowInformation?.WebChannel?.Name ?? string.Empty;

        _episodeName = episode.Name!;
        _episodeSeason = episode.Season ?? 0;
        _episodeNumber = episode.Number ?? 0;
        _episodeImgSrc = episode.Images is not null ? episode.Images!.Original! : string.Empty;
        _missingImg = _episodeImgSrc.IsNullOrWhiteSpace() ? " placeholder placeholder-lg placeholder-wave episodeImgSrc" : " popup";

        string? rawAirTime = episode.AirTime.IsNullOrWhiteSpace() ? airStamp.ToString() : episode.AirTime!;

        _episodeAirTime = DateTime.Parse(rawAirTime ?? string.Empty).ToString("h:mm tt");

        //_episodeAirTime = DateTime.Parse(episode.AirTime!.IsNullOrWhiteSpace() ? airStamp.ToString() : episode.AirTime!).ToString(@"h\:mm tt");
        _officialSite = _showInformation?.OfficialSite ?? string.Empty;

        _episodeRating = episode.Rating?.Average is null ? "No rating" : episode.Rating.Average.Value.ToString();
        _episodeAirDate = airDate!;
        _episodeRuntime = (episode.Runtime is null ? _showInformation?.AverageRuntime.ToString() : episode.Runtime.ToString()) ?? string.Empty;

        _summaryDisplay = new(_showEpisode.Summary ?? "<b>Series summary:</b></br>" + _showInformation?.Summary ?? string.Empty);
        if (_showEpisode.Images is null)
            _missingImg = " placeholder placeholder-lg placeholder-wave episodeImgSrc";
        else
            _missingImg = _episodeImgSrc.IsNullOrWhiteSpace() ? " placeholder placeholder-lg placeholder-wave episodeImgSrc" : " popup";

        return Task.CompletedTask;
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
