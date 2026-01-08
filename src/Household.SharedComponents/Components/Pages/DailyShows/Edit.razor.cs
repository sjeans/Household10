using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Messages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Serilog;
using Alert = Household.SharedComponents.Components.Shared.Modals;

namespace Household.SharedComponents.Components.Pages.DailyShows;

public partial class Edit : ComponentBase
{
    /*[PersistentState(AllowUpdates = true, RestoreBehavior = RestoreBehavior.SkipLastSnapshot)]*/

    [Parameter]
    public int Id { get; set; }

    [Inject] protected IPageHistoryState PageHistory { get; set; } = default!;
    [Inject] public IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    [Inject] private AuthenticationStateProvider Auth { get; set; } = default!;
    [Inject] public ICacheService<List<TVShowDto>> Cache { get; set; } = default!;

    private ICacheService<List<TVShowDto>> _cache = default!;
    private const string ACCESS_TOKEN_KEY = CacheKeys.DailyShowsKey;
    private bool IsEnabled { get; set; } = true;

    private Shared.Loader.Loading _loadingIndicator = default!;

    public required EditContext? EditContextRef { get; set; }

    private TVShowDto? _tVShow;
    private string _message = string.Empty;
    private string _showName = string.Empty;

    private UserIpDto _userIp = default!;
    private TVShowDto? _originalTvShow;
    private Alert.Notification _notification = new();
    private Notification _alertNotification = new();
    private HttpClient? _client;
    private ILogger _logger = default!;
    private bool _editContextInitialized = false;
    private int _id;

    protected override async Task OnParametersSetAsync()
    {
        _id = Id;
        _logger = Logger.ForContext<Edit>();
        _logger.Information("Initializing show editing.");
        _cache = Cache;
        PageHistory.AddPageToHistory("/dailyshows");

        AuthenticationState st = await Auth.GetAuthenticationStateAsync();

        bool isAuthenticated = st.User.Identity?.IsAuthenticated ?? false;

        if (isAuthenticated)
        {
            if (_tVShow is null)
            {
                _client ??= ApiService.HttpClient;

                _userIp = await GetUserIpDetailsAsync();
                _logger.Information("{msg}", _userIp.LogMessage);

                await _loadingIndicator.ShowAsync();
                await GetShowDetailsAsync();

                EditContextRef ??= new EditContext(_tVShow!);

                if (!_editContextInitialized && EditContextRef is not null)
                {
                    EditContextRef.OnFieldChanged += EditContext_OnFieldChanged;
                    _editContextInitialized = true;
                }

                ValidationContext validationContext = new(_tVShow!);
                await _loadingIndicator.HideAsync();
            }
        }
    }

    private async Task GetShowDetailsAsync()
    {
        if (_client == null)
        {
            _logger.Warning("Cannot make client calls for data!");
            return;
        }

        try
        {
            _logger.Information("Retrieving show to edit.");

            HttpResponseMessage responseMessage = await _client.GetAsync($"api/shows/details/{_id}");

            if (responseMessage.IsSuccessStatusCode)
            {

                await using Stream stream = await responseMessage.Content.ReadAsStreamAsync(new());
                Result<TVShowDto> result = await JsonDeserializer.TryDeserializeAsync<TVShowDto>(stream, new());

                if (!result.IsSuccess)
                {
                    _logger.Error("Failed to deserialize: {msg}", result.Error);
                    return;
                }

                TVShowDto tvShow = result.Value!;

                if (tvShow is not null)
                {
                    _originalTvShow = new()
                    {
                        DayOfWeek = tvShow.DayOfWeek,
                        Description = tvShow.Description,
                        Episodes = tvShow.Episodes,
                        Id = tvShow.Id,
                        IsCompleted = tvShow.IsCompleted,
                        IsCompletedSeason = tvShow.IsCompletedSeason,
                        Name = tvShow.Name,
                        Rating = tvShow.Rating,
                        Season = tvShow.Season,
                        StartDate = tvShow.StartDate,
                        StreamingId = tvShow.StreamingId,
                        StreamingName = tvShow.StreamingName,
                        StreamingDescription = tvShow.StreamingDescription,
                        StreamingSubscription = tvShow.StreamingSubscription,
                        Time = tvShow.Time,
                    };
                    _tVShow = tvShow;
                    _showName = tvShow.Name;
                }
            }
            else
                _tVShow = new();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered error retrieving show to edit. Error: {errMsg}", ex.Message);
        }

        return;
    }

    private async Task<UserIpDto> GetUserIpDetailsAsync()
    {
        UserIpDto userIp = new();
        if (_client != null)
        {
            HttpResponseMessage response = await _client.GetAsync("api/UserIpService/GetIpAddress");
            if (response.IsSuccessStatusCode)
            {
                userIp = JsonConvert.DeserializeObject<UserIpDto>(response.Content.ReadAsStringAsync().Result) ?? new();
                _logger.Information("Retrieved ip information.");

                userIp.Visible = string.Empty;
                userIp.DisableButton = string.Empty;
                userIp.CanSave = false;
                userIp.CanShow = string.Empty;
            }
        }
        else
            _logger.Error("Failed to retrieve user IP details.");

        return userIp;
    }

    private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        _logger.Information("The following {type} field {fieldName} was updated by {ip}", e.FieldIdentifier.Model.GetType().Name, e.FieldIdentifier.FieldName, _userIp.IpAddress);
    }

    //protected static Dictionary<string, object> HandyFunction()
    //{
    //    Dictionary<string, object> dict = new()
    //    {
    //        { "autocomplete", true }
    //    };
    //    return dict;
    //}

    //protected void EpisodeUpdate(int newEpisode) => UpdateProperty("Episodes", newEpisode);

    //protected void SeasonUpdate(int newSeason) => UpdateProperty("Season", newSeason);

    //protected void SubscriptionUpdate(int newSubscription) => UpdateProperty("StreamingId", newSubscription);

    //protected void WeekDayUpdate(DayOfWeek newWeekDay) => UpdateProperty("DayOfWeek", (int)newWeekDay);

    //protected async void UpdateProperty(string propertyName, int newValue)
    //{
    //    if (propertyName == "Episodes")
    //        _tVShow!.Episodes = newValue;
    //    else if (propertyName == "Season")
    //        _tVShow!.Season = (Seasons)newValue;
    //    else if (propertyName == "StreamingId")
    //    {
    //        _tVShow!.StreamingId = newValue;
    //        if (_client != null)
    //        {
    //            StreamingServiceDto? newSubscription = await _client.GetFromJsonAsync<StreamingServiceDto>($"api/subscriptions/{newValue}");
    //            if (newSubscription != null)
    //            {
    //                _tVShow.StreamingDescription = newSubscription.Description ?? string.Empty;
    //                _tVShow.StreamingName = newSubscription.Name;
    //                _tVShow.StreamingSubscription = newSubscription.Subscription ?? string.Empty;
    //            }
    //        }
    //    }
    //    else if (propertyName == "DayOfWeek")
    //        _tVShow!.DayOfWeek = (DayOfWeek)newValue;

    //    EditContextRef!.NotifyFieldChanged(EditContextRef.Field(propertyName));
    //}

    //protected static void ValidateProperty(ValidatorEventArgs eventArgs)
    //{
    //    bool selection = int.TryParse((string?)eventArgs.Value, out _);
    //    eventArgs.Status = selection ? ValidationStatus.Success : ValidationStatus.Error;
    //}

    //protected async Task DeleteShow()
    //{
    //    if (_client == null)
    //        return;

    //    try
    //    {
    //        _logger.LogInformation("{ip} is deleting the show {name}.", _userIp.IpAddress, _tVShow.Name);

    //        HttpResponseMessage response = await _client.DeleteAsync($"api/shows/removeshow/{_tVShow.Id}");
    //        string responseString = await response.Content.ReadAsStringAsync();

    //        _logger.LogInformation("{ip} is deleted show: {status}.", _userIp.IpAddress, response.IsSuccessStatusCode);
    //        _message = response.StatusCode.ToString();

    //        if (response.IsSuccessStatusCode)
    //        {
    //            _notification?.Show(1, true, $"You have successfully deleted the show {_tVShow.Name}!");
    //        }
    //        else
    //            _logger.LogInformation("{msg}", responseString);


    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "{ip} encountered an error deleting show. Error: {errMsg}", _userIp.IpAddress, ex.GetInnerMessage());
    //    }
    //}

    //protected async Task HandleInvalid()
    //{
    //    await base.OnInitializedAsync();
    //    //try
    //    //{
    //    //    var i = 0;
    //    //    i ++;
    //    //}
    //    //catch(Exception ex)
    //    //{
    //    //    Console.WriteLine(ex.Message);
    //    //}
    //}

    protected async Task HandleSubmit(EditContext editContext)
    {
        if (_client == null)
        {
            _message = "Cannot make client call to retrieve data!";
            _alertNotification.Show(3, true, _message);
            return;
        }

        if (!editContext.Validate())
        {
            _message = string.Join($"{Environment.NewLine}", editContext.GetValidationMessages());
            _logger.Error("Validation error encountered: {erMsg}", _message);
            _alertNotification?.Show(2, true, _message);
            return;
        }

        if (editContext.IsModified())
        {
            TVShowDto? updatedShow = editContext.Model as TVShowDto;

            if (updatedShow?.StreamingId > 0)
            {
                bool isJsonEqual = updatedShow.JsonCompare(_originalTvShow!);

                if (isJsonEqual) // no changes
                {
                    _message = "No changes found! Please make changes to the show before saving.";
                    //_snackbarNotification.ShowAsync(2, true, Message);
                    _alertNotification?.Show(2, true, _message);
                }
                else
                {
                    // changes
                    _logger.Information("{ip} is updating show.", _userIp.IpAddress);
                    HttpResponseMessage response = await _client.PutAsJsonAsync("api/shows/updateshow", updatedShow);

                    _message = response.StatusCode.ToString();
                    _logger.Information("Update from {ip} success: {success}.", _userIp.IpAddress, response.StatusCode);
                    if (response.IsSuccessStatusCode)
                    {
                        _message = "You have successfully updated the show!";
                        _notification?.Show(1, true, _message);
                    }

                    _logger.Information("Expired cache {success}", await _cache.ExpireAsync(ACCESS_TOKEN_KEY));
                }
            }
            else
            {
                _message = "Invalid streaming service selected!";
                _alertNotification?.Show(3, true, _message);
            }
        }
        else
        {
            _message = "No changes found! Please make changes to the show before saving.";
            _alertNotification?.Show(2, true, _message);
        }
    }
}
