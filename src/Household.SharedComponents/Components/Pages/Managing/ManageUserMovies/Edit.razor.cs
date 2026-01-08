
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Pages.Managing.ManageUserMovies;

public partial class Edit : ComponentBase
{
    [Parameter]
    public int Id { get; set; }

    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private Loading _loadingIndicator = default!;

    private UserIpDto _userIp = default!;
    private HttpClient? _client;
    private ILogger _logger = default!;

    private MovieInfoDto _userMovie = default!;

    private string _message = string.Empty;
    private string _movieName = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _logger = Logger.ForContext<Edit>();
        _logger.Information("Initializing user movies");

        _client = ApiService.HttpClient;

        _userIp = await GetUserIpDetails();
        //_visible = _userIp.Visible;
        //_disable = _userIp.DisableButton;

        await _loadingIndicator.ShowAsync();
        await GetUserMovieAsync();
        await _loadingIndicator.HideAsync();
    }

    private async Task GetUserMovieAsync()
    {
        if (_client == null)
        {
            _message = "Cannot make client call to retrieve data.";
            _logger.Error("{msg}", _message);
            return;
        }

        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken cancellationToken = cts.Token;

        try
        {
            HttpResponseMessage response = await _client.GetAsync($"api/movieinfo/{Id}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<MovieInfoDto> result = await JsonDeserializer.TryDeserializeAsync<MovieInfoDto>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    _message = $"Failed to desrialize: {result.Error}";
                    _logger.Error("{_}", _message);
                    return;
                }

                MovieInfoDto? userMovie = result.Value;

                if (userMovie != null)
                {
                    _userMovie = userMovie;
                    _movieName = userMovie.Title ?? string.Empty;
                }
                else
                _message = "No user movies...";

            }
        }
        catch (OperationCanceledException)
        {
            _logger.Warning("Get user movie information operation was cancelled by user.");
        }
        catch (HttpRequestException hrex)
        {
            _logger.Error(hrex, hrex.GetInnerMessage());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered an Error in GetUserMoviesAsync: {errMsg}", ex.GetInnerMessage());
        }
        finally
        {
            cts.Dispose();
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
