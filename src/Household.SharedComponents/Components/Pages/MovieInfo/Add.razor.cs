using System.Net.Http.Json;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Serilog;

namespace Household.SharedComponents.Components.Pages.MovieInfo;

public partial class Add : ComponentBase
{
    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private ICacheService<List<MovieInfoDto>> Cache { get; set; } = default!;

    public MovieInfoDto MovieInfo { get; set; } = default!;
    public string MovieTitle { get; set; } = string.Empty;
    public string Message { get; set; } = default!;

    private ICacheService<List<MovieInfoDto>> _cache = default!;
    private const string ACCESS_TOKEN_KEY = CacheKeys.MoviesKey;

    private HttpClient? _client;
    private MovieInfoDto? _originalMovieInfo;
    private SuccessNotification _notification = new();
    private ILogger _logger = default!;
    //private EditContext _editContext;

    //public Add()
    //{
    //    MovieInfo = new MovieInfoDto();
    //    Message = string.Empty;
    //}

    protected override void OnInitialized()
    {
        _logger = Logger.ForContext<Add>();
        _logger.Information("Initializing add new movie.");
        PageHistoryState.AddPageToHistory("/movieinfo/index");
        _cache = Cache;
        MovieInfo = new();
        _originalMovieInfo = new()
        {
            CheckedoutTo =  MovieInfo.CheckedoutTo,
            Checkout = MovieInfo.Checkout,
            Collectible = MovieInfo.Collectible,
            Description = MovieInfo.Description,
            DigitalDownload = MovieInfo.DigitalDownload,
            DiskNum = MovieInfo.DiskNum,
            DownloadDate = MovieInfo.DownloadDate,
            Downloaded = MovieInfo.Downloaded,
            DvdType = MovieInfo.DvdType,
            ExpirationDate = MovieInfo.ExpirationDate,
            FirstName = MovieInfo.FirstName,
            HasDownload = MovieInfo.HasDownload,
            Id = MovieInfo.Id,
            Is3D = MovieInfo.Is3D,
            Is4K = MovieInfo.Is4K,
            LastName = MovieInfo.LastName,
            Name = MovieInfo.Name,
            Title = MovieInfo.Title,
            UserInfo = MovieInfo.UserInfo,
        };
        _client = ApiService.HttpClient;
    }

    protected async Task HandleSubmit(EditContext editContext)
    {
        if (_client == null)
            return;

        if (editContext.IsModified())
        {
            if (editContext.Validate())
            {
                MovieInfoDto newMovieInfo = (MovieInfoDto)editContext.Model;

                bool isJsonEqual = newMovieInfo.JsonCompare(_originalMovieInfo!);

                if (isJsonEqual) // no changes
                    Message = "No changes found!";
                else
                {
                    // changes
                    _logger.Information("Adding new movie.");
                    HttpResponseMessage response = await _client.PostAsJsonAsync("api/MovieInfo/", newMovieInfo);

                    Message = response.StatusCode.ToString();
                    _logger.Information("Add success: {success}", Message);
                    if (response.IsSuccessStatusCode)
                    {
                        _notification.Show("You have successfully updated the movie!");
                        _logger.Information("Expired cache {success}", await _cache.ExpireAsync(ACCESS_TOKEN_KEY));
                    }

                }
            }
            else
                editContext.SetFieldCssClassProvider(new CustomCssProvider());

        }
        else
            Message = "No changes found!";

    }

    private class CustomCssProvider : FieldCssClassProvider
    {
        public override string GetFieldCssClass(EditContext editContext,
            in FieldIdentifier fieldIdentifier)
        {
            bool isValid = !editContext.GetValidationMessages(fieldIdentifier).Any();

            if (editContext.IsModified(fieldIdentifier))
                return isValid ? "" : " invalid";
            else
                return isValid ? "" : " invalid";

        }
    }
}
