using System.Net.Http.Json;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared;
using Household.SharedComponents.Components.Shared.Loader;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Serilog;

namespace Household.SharedComponents.Components.Pages.MovieInfo;

public partial class Edit
{
    [Parameter]
    public int Id { get; set; }

    [Parameter]
    public MovieInfoDto MovieInfo { get; set; } = new();

    [Parameter]
    public bool IsEnabled { get; set; } = true;

    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] ILogger Logger { get; set; } = default!;

    [Inject] private AuthenticationStateProvider Auth { get; set; } = default!;
    [Inject] private ICacheService<List<MovieInfoDto>> Cache { get; set; } = default!;

    private ICacheService<List<MovieInfoDto>> _cache = default!;
    private const string ACCESS_TOKEN_KEY = CacheKeys.MoviesKey;

    private string _movieTitle = string.Empty;
    private string? _message = default!;

    private Loading _loadingIndicator = default!;

    private HttpClient? _client;
    private ILogger _logger = default!;
    private MovieInfoDto? _originalMovieInfo;
    private SuccessNotification _notification = new();

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<Edit>();
        _logger.Information("Initializing edit movie.");
        _client = ApiService.HttpClient;
        _cache = Cache;
        PageHistoryState.AddPageToHistory("/movieinfo/index");

        //await loadingIndicator.Show();
        AuthenticationState st = await Auth.GetAuthenticationStateAsync();

        bool isAuthenticated = st.User.Identity?.IsAuthenticated ?? false;

        if (isAuthenticated)
        {
            await GetDailyShowsAsync();
            //await loadingIndicator.Hide();
        }
    }

    private async Task GetDailyShowsAsync()
    {
        if (_client == null)
        {
            _logger.Warning("Cannot make client calls for data!");
            return;
        }

        try
        {
            _logger.Information("Retrieving movie to edit.");
            MovieInfoDto? movieInfo = await _client.GetFromJsonAsync<MovieInfoDto>($"api/movieinfo/{Id}");

            if (movieInfo != null)
            {
                await _loadingIndicator.ShowAsync();
                _originalMovieInfo = new()
                {
                    Id = movieInfo.Id,
                    Title = movieInfo.Title,
                    DvdType = movieInfo.DvdType,
                    CheckedoutTo = movieInfo.CheckedoutTo,
                    Checkout = movieInfo.Checkout,
                    Collectible = movieInfo.Collectible,
                    Description = movieInfo.Description,
                    DigitalDownload = movieInfo.DigitalDownload,
                    DiskNum = movieInfo.DiskNum,
                    DownloadDate = movieInfo.DownloadDate,
                    Downloaded = movieInfo.Downloaded,
                    ExpirationDate = movieInfo.ExpirationDate,
                    FirstName = movieInfo.FirstName,
                    LastName = movieInfo.LastName,
                    HasDownload = movieInfo.HasDownload,
                    Is3D = movieInfo.Is3D,
                    Is4K = movieInfo.Is4K,
                    Name = movieInfo.Name,
                    UserInfo = movieInfo.UserInfo,
                };

                MovieInfo = movieInfo;
                _movieTitle = movieInfo.Title;

                movieInfo = null;
                await _loadingIndicator.HideAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered error retrieving movie to edit. Error: {errMsg}", ex.GetInnerMessage());
        }
    }

    protected async Task HandleSubmit(EditContext editContext)
    {
        if (_client == null)
            return;

        if (editContext.IsModified())
        {
            MovieInfoDto updatedMovieInfo = (MovieInfoDto)editContext.Model;

            if (editContext.Validate() && updatedMovieInfo.Id != 0)
            {
                bool isJsonEqual = updatedMovieInfo.JsonCompare(_originalMovieInfo!);

                if (isJsonEqual) // no changes updatedMovieInfo == _originalMovieInfo
                    _message = "No changes found!";
                else
                {
                    // changes
                    _logger.Information("Updating movie.");
                    HttpResponseMessage response = await _client.PutAsJsonAsync("api/MovieInfo/UpdateMovie", updatedMovieInfo);

                    _message = response.StatusCode.ToString();
                    _logger.Information("Update success: {success}", _message);
                    if (response.IsSuccessStatusCode)
                    {
                        _notification.Show("You have successfully updated the movie!");
                        _logger.Information("Expired cache {success}", await _cache.ExpireAsync(ACCESS_TOKEN_KEY));
                    }
                }
            }
        }
        else
        {
            _message = "No changes found!";
            _notification.Show(_message);
        }
    }
}
