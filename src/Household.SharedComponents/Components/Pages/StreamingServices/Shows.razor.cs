using System.Net.Http.Headers;
using System.Text;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Pages.StreamingServices;

public partial class Shows
{
    [Parameter]
    public int Id { get; init; }

    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private string? _serviceName;
    private string _noneToday = string.Empty;

    private List<TVShowDto>? _activeDailyShows = default!;
    private List<TVShowDto>? _inactiveDailyShows = default!;
    private List<TVShowDto>? _overShows = default!;
    private List<TVShowInformationDto>? _activeShowInformation = default!;

    private Loading _loadingIndicator = default!;

    private string _disable = default!;
    private string selectedTab = "ShowsToWatch";
    private HttpClient? _client;
    private ILogger _logger = default!;
    private List<TVShowDto> _allShows = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _logger = Logger.ForContext<Shows>();
        _logger.Information("Initializing show detail.");
        _client = ApiService.HttpClient;

        if (_allShows.Count == 0)
        {
            //UserIpService.GetUserIP();

            await GetServicesShowsByIdAsync();
            await _loadingIndicator.ShowAsync();
            await GetShowInformationAsync();
            await _loadingIndicator.HideAsync();

            //_visible = UserIpService.Visible;
            //_disable = UserIpService.DisableButton;
            //_logger.LogInformation("{msg}", UserIpService.LogMessage);

            PageHistoryState.AddPageToHistory("/streamingservices/index");
        }

        if(_allShows.Count == 0)
        {
            _noneToday = "No shows for this service...";
        }
    }

    private Task OnSelectedTabChanged(string name)
    {
        selectedTab = name;

        return Task.CompletedTask;
    }

    private async Task GetServicesShowsByIdAsync()
    {
        if (_client == null)
        {
            _noneToday = "Cannot make client calls for data!";
            return;
        }

        try
        {
            _logger.Information("Retrieving streaming service shows for display."/*, UserIpService.IpAddress*/);
            HttpResponseMessage response = await _client.GetAsync($"api/subscriptions/shows/{Id}");

            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync();
            Result<List<TVShowDto>> result = await JsonDeserializer.TryDeserializeAsync<List<TVShowDto>>(stream, new());

            if(!result.IsSuccess)
            {
                _noneToday = $"Failed to deserialize: {result.Error}";
                _logger.Error(_noneToday);
                return;
            }

            List<TVShowDto>? streamingServices = result.Value;

            _logger.Information("Retrieved {count} shows for {serviceName} streaming service.", /*UserIpService.IpAddress,*/ streamingServices?.Count, streamingServices?.FirstOrDefault()?.StreamingName);

            _serviceName = streamingServices?.FirstOrDefault()?.StreamingName;

            if (streamingServices != null && streamingServices.Count > 0)
            {
                _activeDailyShows = [.. streamingServices.Where(s => !s.IsCompleted && !s.IsCompletedSeason)];
                _inactiveDailyShows = [.. streamingServices.Where(s => !s.IsCompleted && s.IsCompletedSeason)];
                _overShows = [.. streamingServices.Where(s => s.IsCompleted)];
            }

            _allShows = streamingServices!;
        }
        catch (HttpRequestException hrex)
        {
            _logger.Error(hrex, hrex.GetInnerMessage());
        }
        catch (Exception ex)
        {
            _logger.Information(ex, "Encountered error retrieving shows. Error: {errMsg}", ex.GetInnerMessage());
        }
    }

    private async Task GetShowInformationAsync()
    {
        string content = JsonConvert.SerializeObject(_activeDailyShows?.Select(tv => tv.Name).ToList());
        byte[] buffer = Encoding.UTF8.GetBytes(content);
        ByteArrayContent byteContent = new(buffer);

        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        try
        {
            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client?.BaseAddress + "api/tvshowinformation/showinformation"),
                Content = byteContent,
            };

            using HttpResponseMessage? response = await _client?.SendAsync(request)!;
            
            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync(new());
            Result<List<TVShowInformationDto>> result = await JsonDeserializer.TryDeserializeAsync<List<TVShowInformationDto>>(stream, new());

            if(!result.IsSuccess)
            {
                _noneToday = $"Failed to deserialize {result.Error}";
                _logger.Error(_noneToday);
                return;
            }

            _activeShowInformation = result.Value;
        }
        catch (HttpRequestException hrex)
        {
            _logger.Error(hrex, hrex.GetInnerMessage());
        }
        catch (Exception ex)
        {
            _logger.Error("Encountered an Error: {errMsg}", ex.GetInnerMessage());
        }
    }
}
