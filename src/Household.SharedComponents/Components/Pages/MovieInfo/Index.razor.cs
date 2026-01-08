//using Blazored.LocalStorage;
using Blazorise;
using Blazorise.DataGrid;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Serilog;

namespace Household.SharedComponents.Components.Pages.MovieInfo;

public partial class Index
{
    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;
    //[Inject] ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] public ICacheService<List<MovieInfoDto>> Cache { get; set; } = default!;

    public List<MovieInfoDto> MovieInfo { get; private set; } = default!;
    protected bool Enabled { get; private set; }

    protected Progress progressRef = default!;
    protected int progress;
    protected int totalMovies;
    private List<MovieInfoDto> _allMovies = [];
    private List<MovieInfoDto>? pagedMovieInfoList;
    private HttpClient? _client;
    private ILogger _logger = default!;

    //private const string STORAGE_KEY = "__DATAGRID_STATE__";
    //private DataGrid<MovieInfoDto> dataGridRef = default!;
    //private IEnumerable<MovieInfoDto> inMemoryData = default!;

    private const string ACCESS_TOKEN_KEY = CacheKeys.MoviesKey;
    protected int intCurrentPage;

    protected override void OnInitialized()
    {
        _logger = Logger.ForContext<Index>();
        _logger.Information("Initializing movie index.");
        _client = ApiService.HttpClient;
        Enabled = true;

        MovieInfo = [];
        totalMovies = MovieInfo.Count;
        progress = 0;
        progressRef = new Progress();

        PageHistoryState.AddPageToHistory("/movieInfo/index");
    }

    private async Task GetAllDvdMovieAsync()
    {
        try
        {
            _allMovies = await Cache.GetOrCreateAsync(ACCESS_TOKEN_KEY, GetAllMoviesAsync, TimeSpan.FromHours(3));

            if (_allMovies is null)
            {
                _allMovies = await GetAllMoviesAsync();
                _logger.Information("Cache miss: Retrieved {count} movies from source.", _allMovies?.Count);
            }
            else
                _logger.Information("Retrieved {count} movies from cache.", _allMovies?.Count);

            _logger.Information("Retrieving movies for display.");

            if(_allMovies is not null)
            {
                List<MovieInfoDto> movies = _allMovies;
                MovieInfo = movies;
                totalMovies = movies.Count;
            }

            _logger.Information("Retrieved {count} movies for display.", _allMovies!.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered error retrieving movies. Error: {errMsg}", ex.Message);
        }
    }

    private async Task<List<MovieInfoDto>> GetAllMoviesAsync()
    {
        List<MovieInfoDto> allMovies = [];
        if (_client == null)
        {
            _logger.Error("Cannot make call to retrieve client data!");
            return allMovies;
        }

        CancellationTokenSource cts = new();
        CancellationToken cancellationToken = cts.Token;

        try
        {
            HttpResponseMessage response = await _client.GetAsync("api/movieinfo");
            if (response.IsSuccessStatusCode)
            {
                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<List<MovieInfoDto>> results = await JsonDeserializer.TryDeserializeAsync<List<MovieInfoDto>>(stream, cancellationToken);

                if (!results.IsSuccess)
                {
                    _logger.Error("{msg}", results.Error);
                }

                allMovies = results.Value!;
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

        return allMovies;
    }

    public async Task LoadMoviesFromService(DataGridReadDataEventArgs<MovieInfoDto> e)
    {
        ArgumentNullException.ThrowIfNull(e);

        /*
        * This can be call to anything like calling an api to load employees.
        * During execution 'LoadingTemplate' will be displayed.
        * If your api call returns empty result, then 'EmptyTemplate' will be displayed,
        * this way you have proper feedback, for when your datagrid is loading or empty.
        */
        progress = 0;
        await InvokeAsync(StateHasChanged);

        await Task.Delay(250);
        progress = 25;
        await InvokeAsync(StateHasChanged);

        await Task.Delay(250);
        progress = 50;
        await InvokeAsync(StateHasChanged);

        await Task.Delay(250);
        progress = 75;
        await InvokeAsync(StateHasChanged);


        await Task.Delay(250);
        progress = 100;
        await InvokeAsync(StateHasChanged);

        Enabled = false;
    }

    private async Task OnReadDataAsync(DataGridReadDataEventArgs<MovieInfoDto> e)
    {
        if (MovieInfo.Count == 0)
            await GetAllDvdMovieAsync();

        List<MovieInfoDto>? response;
        Enabled = true;

        int onPage = e.Page;
        int numberOfItemsToGrab = e.PageSize - 1;
        int numberPerPage = e.PageSize;

        CheckPage(onPage, numberOfItemsToGrab, numberPerPage, out int pageNumberToUse);

        _logger.Information("Paging thru the movies. Page: {page}; Skip: {skip}; Take: {take};", onPage, numberOfItemsToGrab, numberPerPage);
        if (e.ReadDataMode is DataGridReadDataMode.Virtualize)
            response = [.. MovieInfo.Skip(e.VirtualizeOffset).Take(e.VirtualizeCount)];
        else if (e.ReadDataMode is DataGridReadDataMode.Paging)
            response = [.. MovieInfo.Skip((pageNumberToUse - 1) * numberPerPage).Take(numberPerPage)];
        else
        {
            _logger.Error("Unhandled ReadDataMode!");
            throw new Exception("Unhandled ReadDataMode");
        }

        if (!e.CancellationToken.IsCancellationRequested)
        {
            totalMovies = MovieInfo.Count;
            pagedMovieInfoList = response; // an actual data for the current page

            //inMemoryData = pagedMovieInfoList;
            //await SaveState();
            await LoadMoviesFromService(e);
        }

        await Task.FromResult(0);
    }

    private void CheckPage(int onPage, int numToGrab, int numPerPage, out int pageNumberToUse)
    {
        // Calculate the total number of pages with the initial number of items per page
        int totalPagesInitial = (int)Math.Ceiling((double)totalMovies / numToGrab);

        //// Calculate the total number of pages with the new number of items per page
        //int totalPagesNew = (int)Math.Ceiling((double)totalMovies / numPerPage);

        // Check if navigating from the initial page to the new page is needed
        if (onPage > totalPagesInitial)
        {
            // Calculate the new page number after switching to the new number of items per page
            pageNumberToUse = (int)Math.Ceiling((double)totalMovies / numPerPage);
        }
        else
        {
            pageNumberToUse = onPage;
        }
    }


    //protected async override Task OnAfterRenderAsync(bool firstRender)
    //{
    //    if (firstRender)
    //    {
    //        await LoadState();
    //    }

    //    await base.OnAfterRenderAsync(firstRender);
    //}

    //private async Task ResetState()
    //{
    //    await LocalStorage.RemoveItemAsync(STORAGE_KEY);

    //    var state = new DataGridState<MovieInfoDto>()
    //    {
    //        CurrentPage = 1,
    //        PageSize = 10,
    //    };

    //    await dataGridRef.LoadState(state);
    //}

    //private async Task LoadState()
    //{
    //    DataGridState<MovieInfoDto>? stateFromLocalStorage = await LocalStorage.GetItemAsync<DataGridState<MovieInfoDto>>(STORAGE_KEY);

    //    if (stateFromLocalStorage is not null)
    //    {
    //        //It is of note that we must make sure the reference is contained in the DataGrid Data collection.
    //        if (stateFromLocalStorage.SelectedRow is not null)
    //        {
    //            stateFromLocalStorage.SelectedRow = inMemoryData.FirstOrDefault(x => x.Id == stateFromLocalStorage.SelectedRow.Id) ?? new();
    //        }
            
    //        if (stateFromLocalStorage.EditItem is not null)
    //        {
    //            stateFromLocalStorage.EditItem = inMemoryData.FirstOrDefault(x => x.Id == stateFromLocalStorage.EditItem.Id) ?? new();
    //        }

    //        await dataGridRef.LoadState(stateFromLocalStorage);
    //        return;
    //    }
    //}

    //private async Task SaveState()
    //{
    //    DataGridState<MovieInfoDto> state = await dataGridRef.GetState();
    //    await LocalStorage.SetItemAsync(STORAGE_KEY, state);
    //}
}
