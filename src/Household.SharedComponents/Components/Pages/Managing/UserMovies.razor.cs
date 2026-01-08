using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;
using Household.SharedComponents.Components.Shared.Loader;

namespace Household.SharedComponents.Components.Pages.Managing;

public partial class UserMovies
{
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private Loading _loadingIndicator = default!;

    private List<UserMovie>? _allUserMovies = default!;
    private UserIpDto _userIp = default!;
    private HttpClient? _client;
    private ILogger _logger = default!;

    Dictionary<int, (string, int, string)> _pairs = new();

    private string? _noneToday;

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<UserMovies>();
        _logger.Information("Initializing user movies");
        _client = ApiService.HttpClient;

        _userIp = await GetUserIpDetails();
        //_visible = _userIp.Visible;
        //_disable = _userIp.DisableButton;

        await _loadingIndicator.ShowAsync();
        await GetUserMoviesAsync();
        await _loadingIndicator.HideAsync();
    }

    private async Task GetUserMoviesAsync()
    {
        if (_client == null)
        {
            _noneToday = "Cannot make client call to retrieve data.";
            _logger.Error("{msg}", _noneToday);
            return;
        }

        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken cancellationToken = cts.Token;

        try
        {
            HttpResponseMessage response = await _client.GetAsync("api/usermovie", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<List<UserMovie>> result = await JsonDeserializer.TryDeserializeAsync<List<UserMovie>>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    _noneToday = $"Failed to desrialize: {result.Error}";
                    _logger.Error("{_}", _noneToday);
                    return;
                }

                List<UserMovie>? userMovieList = result.Value;

                if (userMovieList != null)
                {
                    _allUserMovies = userMovieList;
                    foreach (var item in userMovieList)
                    {
                        _pairs.Add(item.MovieId, (await GetMovieNameAsync(item.MovieId), item.UserId, await GetUserNameAsync(item.UserId)));
                    }
                }
                //if (userMovieList != null)
                //    _allUserMovies = userMovieList;
                else
                    _noneToday = "No user movies...";

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

    private string GetMovieName(int movieId)
    {
        if (_client == null)
        {
            _noneToday = "Cannot retrieve data.";
            _logger.Error("{msg}", _noneToday);
        }

        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken cancellationToken = cts.Token;

        try
        {
            HttpResponseMessage response = _client!.GetAsync($"api/movieinfo/{movieId}", cancellationToken).Result;

            if (response.IsSuccessStatusCode)
            {
                using Stream stream = response.Content.ReadAsStreamAsync(cancellationToken).Result;
                Result<MovieInfoDto> result = JsonDeserializer.TryDeserializeAsync<MovieInfoDto>(stream, cancellationToken).Result;

                if (!result.IsSuccess)
                {
                    _noneToday = $"Failed to desrialize: {result.Error}";
                    _logger.Error("{_}", _noneToday);
                    return _noneToday;
                }

                MovieInfoDto? movie = result.Value;

                if (movie != null)
                    return movie.Title;
                else
                    return "Unknown";

            }
        }
        catch (OperationCanceledException)
        {
            _logger.Warning("Get movie name operation was cancelled by user.");
        }
        catch (HttpRequestException hrex)
        {
            _logger.Error(hrex, hrex.GetInnerMessage());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered an Error in GetMovieNameAsync: {errMsg}", ex.GetInnerMessage());
        }
        finally
        {
            cts.Dispose();
        }

        return string.Empty;
    }

    private async Task<string> GetMovieNameAsync(int movieId)
    {
        if (_client == null)
        {
            _noneToday = "Cannot retrieve data.";
            _logger.Error("{msg}", _noneToday);
        }

        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken cancellationToken = cts.Token;

        try
        {
            HttpResponseMessage response = await _client!.GetAsync($"api/movieinfo/{movieId}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<MovieInfoDto> result = await JsonDeserializer.TryDeserializeAsync<MovieInfoDto>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    _noneToday = $"Failed to desrialize: {result.Error}";
                    _logger.Error("{_}", _noneToday);
                    return _noneToday;
                }

                MovieInfoDto? movie = result.Value;

                if (movie != null)
                    return movie.Title;
                else
                    return "Unknown";

            }
        }
        catch (OperationCanceledException)
        {
            _logger.Warning("Get movie name operation was cancelled by user.");
        }
        catch (HttpRequestException hrex)
        {
            _logger.Error(hrex, hrex.GetInnerMessage());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered an Error in GetMovieNameAsync: {errMsg}", ex.GetInnerMessage());
        }
        finally
        {
            cts.Dispose();
        }

        return string.Empty;
    }

    private string GetUserName(int userId)
    {
        if (_client == null)
        {
            _noneToday = "Cannot retrieve data.";
            _logger.Error("{msg}", _noneToday);
        }

        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken cancellationToken = cts.Token;

        try
        {
            HttpResponseMessage response = _client!.GetAsync($"api/user/{userId}", cancellationToken).Result;

            if (response.IsSuccessStatusCode)
            {
                using Stream stream = response.Content.ReadAsStreamAsync(cancellationToken).Result;
                Result<User> result = JsonDeserializer.TryDeserializeAsync<User>(stream, cancellationToken).Result;

                if (!result.IsSuccess)
                {
                    _noneToday = $"Failed to desrialize: {result.Error}";
                    _logger.Error("{_}", _noneToday);
                    return _noneToday;
                }

                User? user = result.Value;

                if (user != null)
                    return string.Concat(user.FirstName, " ", user.LastName);
                else
                    return "Unknown";

            }
        }
        catch (OperationCanceledException)
        {
            _logger.Warning("Get user name operation was cancelled by user.");
        }
        catch (HttpRequestException hrex)
        {
            _logger.Error(hrex, hrex.GetInnerMessage());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered an Error in GetUserNameAsync: {errMsg}", ex.GetInnerMessage());
        }
        finally
        {
            cts.Dispose();
        }

        return string.Empty;
    }

    private async Task<string> GetUserNameAsync(int userId)
    {
        if (_client == null)
        {
            _noneToday = "Cannot retrieve data.";
            _logger.Error("{msg}", _noneToday);
        }

        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken cancellationToken = cts.Token;

        try
        {
            HttpResponseMessage response = await _client!.GetAsync($"api/user/{userId}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<User> result = await JsonDeserializer.TryDeserializeAsync<User>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    _noneToday = $"Failed to desrialize: {result.Error}";
                    _logger.Error("{_}", _noneToday);
                    return _noneToday;
                }

                User? user = result.Value;

                if (user != null)
                    return string.Concat(user.FirstName, " ", user.LastName);
                else
                    return "Unknown";

            }
        }
        catch (OperationCanceledException)
        {
            _logger.Warning("Get user name operation was cancelled by user.");
        }
        catch (HttpRequestException hrex)
        {
            _logger.Error(hrex, hrex.GetInnerMessage());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered an Error in GetUserNameAsync: {errMsg}", ex.GetInnerMessage());
        }
        finally
        {
            cts.Dispose();
        }

        return string.Empty;
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
