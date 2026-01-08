using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Pages.MovieInfo;

public partial class DvdMovies
{
    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private Loading _loadingIndicator = default!;

    private List<MovieInfoDto> _allMoviesInfos = default!;
    private List<DvdMovieInformation> _allDvdMovies = default!;
    private List<DvdMovieInformationDto> _allDvdMovieDtos = default!;
    private List<DvdMovieInformationDto> _pagedDvdMovieDtos = default!;

    private List<int> _movieInfoIds = default!;
    protected int _totalMovies = default!;

    private HttpClient? _client = default!;
    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _logger = Logger.ForContext<DvdMovies>();
        _logger.Information("Initializing movie index.");
        _client = ApiService.HttpClient;
        await GetAllDvdMovieIds();
        await GetAllDvdMovies(_movieInfoIds);
        await BuildDisplayDto();

        PageHistoryState.AddPageToHistory("/movieinfo/dvdmovies");
        await _loadingIndicator.HideAsync();
    }

    private void CheckPage(int onPage, int numToGrab, int numPerPage, out int pageNumberToUse)
    {
        // Calculate the total number of pages with the initial number of items per page
        int totalPagesInitial = (int)Math.Ceiling((double)_totalMovies / numToGrab);

        //// Calculate the total number of pages with the new number of items per page
        //int totalPagesNew = (int)Math.Ceiling((double)totalMovies / numPerPage);

        // Check if navigating from the initial page to the new page is needed
        if (onPage > totalPagesInitial)
        {
            // Calculate the new page number after switching to the new number of items per page
            pageNumberToUse = (int)Math.Ceiling((double)_totalMovies / numPerPage);
        }
        else
        {
            pageNumberToUse = onPage;
        }
    }

    private async Task GetAllDvdMovieIds()
    {
        if (_client is null)
        {
            _logger.Error("Cannot make client call to retrieve data!");
            return;
        }

        CancellationTokenSource cts = new();
        CancellationToken cancellationToken = cts.Token;

        try
        {
            _logger.Information("Retrieving movies for display.");

            HttpResponseMessage response = await _client.GetAsync("api/movieinfo");
            if (response.IsSuccessStatusCode)
            {
                await _loadingIndicator.ShowAsync();
                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<List<MovieInfoDto>> results = await JsonDeserializer.TryDeserializeAsync<List<MovieInfoDto>>(stream, cancellationToken);
                
                if (!results.IsSuccess)
                {
                    _logger.Error("{msg}", results.Error);
                    return;
                }

                _allMoviesInfos = results.Value!;

                List<int> movieInfoIds = [.. results.Value!.Select(mv => mv.Id)];

                if (movieInfoIds is not null)
                {
                    _movieInfoIds = movieInfoIds;
                    //await GetAllDvdMovies(movieInfoIds);
                }

                _totalMovies = movieInfoIds!.Count;
                _logger.Information("Retrieved {count} movies for display.", _totalMovies);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Warning("Get movie information operation was cancelled by user.");
        }
        catch (HttpRequestException hrex)
        {
            _logger.Error(hrex, "{msg}", hrex.GetInnerMessage());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered error retrieving movies. Error: {errMsg}", ex.Message);
        }
        finally
        {
            cts.Dispose();
        }

        return;
    }

    private async Task GetAllDvdMovies(List<int> ids)
    {
        if(_client is null)
        {
            _logger.Error("Cannot make client call to retrieve data!");
            return;
        }

        CancellationTokenSource cts = new();
        CancellationToken cancellationToken = cts.Token;

        string content = JsonConvert.SerializeObject(ids);
        byte[] buffer = Encoding.UTF8.GetBytes(content);
        ByteArrayContent byteContent = new(buffer);

        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpResponseMessage response = await _client.PostAsync("api/dvdmovieinformation/getallmovieinformationids", byteContent);

        if (response.IsSuccessStatusCode)
        {
            await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            Result<List<DvdMovieInformation>> result = await JsonDeserializer.TryDeserializeAsync<List<DvdMovieInformation>>(stream, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.Error("Failed to deserialize: {obj}", result.Error);
                return;
            }

            List<DvdMovieInformation> dvdMovies = result.Value!;
            _allDvdMovies = dvdMovies;
        }

        //await BuildDisplayDto();
        return;
    }

    private async Task BuildDisplayDto()
    {
        ConcurrentDictionary<int, DvdMovieInformationDto> results = new ();

        ActionBlock<(MovieInfoDto movie, int movieId, int cnt, int identityHash)> actionBlock = new (item =>
        {
            DvdMovieInformationDto dtoItem = new();
            DvdMovieInformation? found = _allDvdMovies.FirstOrDefault(f => f.MovieId == item.movieId);
            
            // inside block
            if (found is not null)
                dtoItem = new()
                {
                    Adult = found is not null && found.Adult,
                    BackdropPath = found is null ? string.Empty : found.BackdropPath,
                    CheckedoutTo = item.movie.CheckedoutTo,
                    Checkout = item.movie.Checkout,
                    Collectible = item.movie.Collectible,
                    Description = item.movie.Description,
                    DigitalDownload = item.movie.DigitalDownload,
                    DiskNum = item.movie.DiskNum,
                    DownloadDate = item.movie.DownloadDate,
                    Downloaded = item.movie.Downloaded,
                    DvdType = item.movie.DvdType,
                    ExpirationDate = item.movie.ExpirationDate,
                    FirstName = item.movie.FirstName,
                    Genres = GetGenres(found?.Genres ?? []),
                    HasDownload = item.movie.HasDownload,
                    HomePage = found is null ? string.Empty : found.HomePage,
                    Id = found is null ? 0 : found.Id,
                    ImdbId = found is null ? string.Empty : found.ImdbId,
                    Is3D = item.movie.Is3D,
                    Is4K = item.movie.Is4K,
                    LastName = item.movie.LastName,
                    MovieCollections = found is null ? new() { Name = "" } : found.MovieCollections,
                    MovieId = item.movieId, // item.movie.Id,
                    Name = item.movie.Name,
                    OriginCountries = found is null ? [] : found.OriginCountries,
                    OriginalLanguage = found is null ? string.Empty : found.OriginalLanguage,
                    OriginalTitle = found is null ? item.movie.Title : found.OriginalTitle,
                    Overview = found is null ? item.movie.Description ?? string.Empty : found.Overview,
                    Popularity = found is null ? 0 : found.Popularity,
                    PosterPath = found is null ? string.Empty : found.PosterPath,
                    ProductionCompanies = found is null ? [] : found.ProductionCompanies,
                    ProductionCountries = found is null ? [] : found.ProductionCountries,
                    Released = found is null ? string.Empty : found.Released,
                    ReleaseDate = GetReleaseDate(found?.ReleaseDate),
                    Revenue = GetRevenue(found?.Revenue),
                    Runtime = GetHoursAndMinutes(found?.Runtime),
                    SpokenLanguages = found is null ? [] : found.SpokenLanguages,
                    TagLine = found is null ? string.Empty : found.TagLine,
                    Title = found is null ? item.movie.Title : found.Title,
                    TmdbId = found is null ? 0 : found.TmdbId,
                    UserInfo = found is null ? new() : item.movie.UserInfo,
                    Video = found is not null && found.Video,
                    VoteAverage = found is null ? 0 : found.VoteAverage,
                    VoteCount = found is null ? 0 : found.VoteCount,
                };
            else
                dtoItem = new()
                {
                    Adult = false,
                    BackdropPath = string.Empty,
                    CheckedoutTo = item.movie.CheckedoutTo,
                    Checkout = item.movie.Checkout,
                    Collectible = item.movie.Collectible,
                    Description = item.movie.Description,
                    DigitalDownload = item.movie.DigitalDownload,
                    DiskNum = item.movie.DiskNum,
                    DownloadDate = item.movie.DownloadDate,
                    Downloaded = item.movie.Downloaded,
                    DvdType = item.movie.DvdType,
                    ExpirationDate = item.movie.ExpirationDate,
                    FirstName = item.movie.FirstName,
                    Genres = GetGenres([]),
                    HasDownload = item.movie.HasDownload,
                    HomePage = string.Empty,
                    Id = 0,
                    ImdbId = string.Empty,
                    Is3D = item.movie.Is3D,
                    Is4K = item.movie.Is4K,
                    LastName = item.movie.LastName,
                    MovieCollections = new() { Name = "" },
                    MovieId = item.movieId, // item.movie.Id,
                    Name = item.movie.Name,
                    OriginCountries = [],
                    OriginalLanguage = string.Empty,
                    OriginalTitle = item.movie.Title,
                    Overview = item.movie.Description ?? string.Empty,
                    Popularity = 0,
                    PosterPath = string.Empty,
                    ProductionCompanies = [],
                    ProductionCountries = [],
                    Released = string.Empty,
                    //ReleaseDate = new(),
                    //Revenue = 0,
                    //Runtime = string.Empty,
                    ReleaseDate = GetReleaseDate(new()),
                    Revenue = GetRevenue(0.0),
                    Runtime = GetHoursAndMinutes(string.Empty),
                    SpokenLanguages = [],
                    TagLine = string.Empty,
                    Title = item.movie.Title,
                    TmdbId = 0,
                    UserInfo = item.movie.UserInfo,
                    Video = false,
                    VoteAverage = 0,
                    VoteCount = 0,
                };

            // Try to add; keep the first occurrence
            bool added = results.TryAdd(dtoItem.MovieId, dtoItem);

            if (!added)
            {
                // Existing entry present, check for mismatched title
                DvdMovieInformationDto existing = results[dtoItem.MovieId];
                if (existing.Title != dtoItem.Title) // Log a warning: first occurrence is preserved
                    _logger.Warning("Duplicate MovieId detected. Existing Title='{existingTitle}', New Title='{newTitle}'", existing.Title, dtoItem.Title);

            }
        }, new() { MaxDegreeOfParallelism = Environment.ProcessorCount });

        int infosCount = 0;
        _allDvdMovieDtos = [];
        _pagedDvdMovieDtos = [];

        foreach (MovieInfoDto movieInfo in _allMoviesInfos)
        {
            MovieInfoDto movieSnapshot = new ()
            {
                CheckedoutTo = movieInfo.CheckedoutTo,
                Checkout = movieInfo.Checkout,
                Collectible = movieInfo.Collectible,
                Description = movieInfo.Description,
                DigitalDownload = movieInfo.DigitalDownload,
                DiskNum = movieInfo.DiskNum,
                DownloadDate = movieInfo.DownloadDate,
                Downloaded = movieInfo.Downloaded,
                DvdType = movieInfo.DvdType,
                ExpirationDate = movieInfo.ExpirationDate,
                FirstName = movieInfo.FirstName,
                HasDownload = movieInfo.HasDownload,
                Id = movieInfo.Id,
                Is3D = movieInfo.Is3D,
                Is4K = movieInfo.Is4K,
                LastName = movieInfo.LastName,
                Name = movieInfo.Name,
                Title = movieInfo.Title,
                UserInfo = movieInfo.UserInfo,
            };

            int sendingId = movieSnapshot.Id;
            int idHash = RuntimeHelpers.GetHashCode(movieSnapshot);
            
            _ = await actionBlock.SendAsync((movieSnapshot, sendingId, infosCount, idHash));

            infosCount++;
        }

        actionBlock.Complete(); // Signal that no more messages will be posted.
        await actionBlock.Completion; // Wait for all messages to be processed.

        List<DvdMovieInformationDto> orderdMovies = [.. results.Values.OrderBy(mv => mv.Title)];

        _allDvdMovieDtos.AddRange(orderdMovies);
        _pagedDvdMovieDtos.AddRange(orderdMovies.Skip(0).Take(10));

        return;
    }

    private static string GetGenres(List<DvdGenre> genres)
    {
        if (genres.Any())
            return string.Join(", ", genres.Select(g => g.Name));

        return "Not listed";
    }

    private static string GetHoursAndMinutes(string? total)
    {
        if (int.TryParse(total, out int totalMinutes))
        {
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;
            return $"{hours}h {minutes}min";
        }

        return "N/A";
    }

    private static string GetReleaseDate(DateTime? releaseDate)
    {
        if (releaseDate.HasValue)
        {
            if(!releaseDate.Value.ToShortDateString().Equals("1/1/0001"))
                return releaseDate.Value.ToShortDateString();

        }
        
        return "N/A";
    }

    private static string GetRevenue(double? revenue)
    {
        if (revenue is null)
            return 0.ToString("C");

        return revenue.Value.ToString("C");
    }

    private static string? GetDvdMovieInfoHomePage(MovieInfoDto infoDto, List<DvdMovieInformation> movieInformations)
    {
        DvdMovieInformation? found = movieInformations.FirstOrDefault(dv => dv.MovieId == infoDto.Id);
        if (found is not null)
            return found.HomePage;

        return string.Empty;
    }

    private static string? GetDvdMovieInfoTagLine(MovieInfoDto infoDto, List<DvdMovieInformation> movieInformations)
    {
        DvdMovieInformation? found = movieInformations.FirstOrDefault(dv => dv.MovieId == infoDto.Id);
        if (found is not null)
            return found.TagLine;

        return !infoDto.Name.IsNullOrWhiteSpace() ? infoDto.Name : string.Empty;
    }

    private void Next()
    {
        int pageSize = 10;
        int onPage = 0;
        int numberOfItemsToGrab = pageSize - 1;
        int numberPerPage = pageSize;

        CheckPage(onPage, numberOfItemsToGrab, numberPerPage, out int pageNumberToUse);

        _pagedDvdMovieDtos = [.. _allDvdMovieDtos.Skip((pageNumberToUse - 1) * numberPerPage).Take(numberPerPage)];
    }
}
