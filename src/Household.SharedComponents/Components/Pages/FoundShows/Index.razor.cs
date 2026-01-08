using System.Text;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Pages.FoundShows;

public partial class Index : ComponentBase
{
    [Parameter]
    public string SearchTerm { get; set; } = string.Empty;

    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private List<TVShowDto>? _foundShows = default!;
    private List<TVShowInformationDto>? _activeShowInformation = default!;
    private string _disable = default!;

    private Loading _loadingIndicator = default!;
    private bool _showInputs = false;

    private string _noneFound = string.Empty;
    private string _selectedTab = "ShowsToWatch";
    private UserIpDto _userIp = default!;
    private HttpClient? _client;
    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<Index>();
        _logger.Information("Initializing found shows index.");
        _client = ApiService.HttpClient;
        _userIp = await GetUserIpDetails();
        _disable = _userIp.DisableButton;
        _logger.Information("{msg}", _userIp.LogMessage);

        await _loadingIndicator.ShowAsync();
        _showInputs = _loadingIndicator.IsVisible;
        await GetShowInformationAsync();
        PageHistoryState.AddPageToHistory("/dailyshows");
        await _loadingIndicator.HideAsync();
        _showInputs = _loadingIndicator.IsVisible;
    }

    private Task OnSelectedTabChanged(string name)
    {
        _selectedTab = name;

        return Task.CompletedTask;
    }

    protected async Task SearchIssuesAsync()
    {
        if (_client == null)
        {
            _noneFound = "Cannot make client calls for data!";
            return;
        }

        try
        {
            _logger.Information("Retrieving search results.");
            HttpResponseMessage responseMessage = await _client.GetAsync("api/shows");

            responseMessage.EnsureSuccessStatusCode();

            await using Stream stream = responseMessage.Content.ReadAsStream(new());
            Result<List<TVShowDto>> result = await JsonDeserializer.TryDeserializeAsync<List<TVShowDto>>(stream, new());

            if (!result.IsSuccess)
            {
                _noneFound = $"Failed to deserialize: {result.Error}";
                _logger.Error(_noneFound);
                return;
            }

            List<TVShowDto>? tvShows = result.Value;

            if (tvShows!.Count != 0)
            {
                _foundShows = tvShows?.Where(search => search.Name.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase)).ToList();
                if (_foundShows!.Count == 0)
                    _noneFound = $"No shows matching: {SearchTerm}";

            }
            else
                _noneFound = $"No shows matching: {SearchTerm}";

        }
        catch (Exception ex)
        {
            _logger.Information(ex, "Encountered error retrieving shows. Error: {errMsg}", ex.GetInnerMessage());
        }
    }

    private async Task GetShowInformationAsync()
    {
        await SearchIssuesAsync();
        string content = JsonConvert.SerializeObject(_foundShows?.Select(tv => tv.Name).ToList());
        byte[] buffer = Encoding.UTF8.GetBytes(content);
        ByteArrayContent byteContent = new(buffer);

        byteContent.Headers.ContentType = new ("application/json");

        try
        {
            HttpRequestMessage request = new ()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client?.BaseAddress + "api/tvshowinformation/showinformation"),
                Content = byteContent,
            };

            using HttpResponseMessage? response = await _client?.SendAsync(request)!;
            response.EnsureSuccessStatusCode();

            await using Stream stream = response.Content.ReadAsStream(new());
            Result<List<TVShowInformationDto>> result = await JsonDeserializer.TryDeserializeAsync<List<TVShowInformationDto>>(stream, new());

            if (!result.IsSuccess)
            {
                _noneFound = $"Failed to deserialize: {result.Error}";
                _logger.Error(_noneFound);
                return;
            }

            _activeShowInformation = result.Value;
        }
        catch (Exception ex)
        {
            _logger.Error("Encountered an Error: {errMsg}", ex.GetInnerMessage());
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
