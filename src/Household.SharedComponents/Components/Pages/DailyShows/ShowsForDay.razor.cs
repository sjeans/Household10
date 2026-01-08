using System.Net.Http.Headers;
using System.Text;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Household.SharedComponents.Components.Shared.Loader;
using Serilog;
using static Household.Shared.Helpers.Common;

namespace Household.SharedComponents.Components.Pages.DailyShows;

public partial class ShowsForDay : ComponentBase
{
    [Parameter]
    public long Id { get; set; }

    [Parameter]
    public int Month { get; set; }

    [Parameter]
    public int Day { get; set; }

    [Parameter]
    public int Year { get; set; }

    [Inject] public required IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private Loading _loadingIndicator = default!;

    private DateTime _today;
    private DayOfWeek _dayName;
    private List<TVShowInformationDto>? _activeShowInformation;
    private List<TVShowDto>? _activeDailyShows;
    private List<TVShowDto>? _inactiveDailyShows;
    private List<TVShowDto>? _overShows;

    private bool _enabled = true;
    private bool _showInputs = true;
    private string _disable = default!;
    private string _noneToday = string.Empty;
    private string _selectedTab = "ShowsToWatch";
    private string _visible = default!;

    private UserIpDto _userIp = default!;
    private List<TVShowDto> _allShows = [];
    private HttpClient? _client = default!;
    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<ShowsForDay>();
        _client = ApiService.HttpClient;
        if (Id > 0)
            _today = new(Id);
        else
            _today = ValidDate();

        _logger.Information("Initializing shows for selected day: {selectedDay} {selectedDate}.", _dayName, _today.ToShortDateString());

        if (_allShows.Count == 0)
        {
            _userIp = await GetUserIpDetails();

            _visible = _userIp.Visible;
            _disable = _userIp.DisableButton;
            _logger.Information("{msg}", _userIp.LogMessage);

            await _loadingIndicator.ShowAsync();
            _showInputs = _loadingIndicator.IsVisible;

            await GetShowInformationAsync();
            _enabled = false;
            await _loadingIndicator.HideAsync();
            _showInputs = _loadingIndicator.IsVisible;
        }
    }

    private Task OnSelectedTabChanged(string name)
    {
        _selectedTab = name;
        return Task.CompletedTask;
    }

    private async Task GetDailyShowsAsync()
    {
        if (_client == null)
        {
            _noneToday = "Cannot make client calls for data!";
            _logger.Warning("{msg}", _noneToday);
            return;
        }

        try
        {
            _dayName = _today.DayOfWeek;
            _logger.Information("{ip} is retrieving shows for selected day: {selectedDay} {selectedDate}.", _userIp.IpAddress, _dayName, _today.ToShortDateString());

            HttpResponseMessage responseMessage = await _client.GetAsync($"api/shows/dayofweek/{(int)_dayName}");

            responseMessage.EnsureSuccessStatusCode();

            await using Stream stream = await responseMessage.Content.ReadAsStreamAsync(new());
            Result<List<TVShowDto>> result = await JsonDeserializer.TryDeserializeAsync<List<TVShowDto>>(stream, new());

            if(!result.IsSuccess)
            {
                _noneToday = $"Failed to deserialize: {result.Error}";
                _logger.Error(_noneToday);
                return;
            }

            List<TVShowDto>? tvShows = result.Value;

            _logger.Information("{ip} retrieved {count} shows for selected day: {selectedDay} {selectedDate}", _userIp.IpAddress, tvShows?.Count, _dayName, _today.ToShortDateString());

            if (tvShows != null && tvShows.Count > 0)
            {
                _allShows = tvShows;
                _activeDailyShows = _allShows.Where(s => s.IsCompleted == false && s.IsCompletedSeason == false).ToList();
                _inactiveDailyShows = _allShows.Where(s => s.IsCompleted == false && s.IsCompletedSeason == true).ToList();
                _overShows = _allShows.Where(s => s.IsCompleted == true).ToList();
            }
            else
                _noneToday = "No shows for today...";

        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered error retrieving show for today. Error {errMsg}", ex.GetInnerMessage());
        }
    }

    private async Task GetShowInformationAsync()
    {
        await GetDailyShowsAsync();

        if (_activeDailyShows is not null)
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

                using HttpResponseMessage response = await _client?.SendAsync(request)!;
                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(new());
                Result<List<TVShowInformationDto>> result = await JsonDeserializer.TryDeserializeAsync<List<TVShowInformationDto>>(stream, new());

                if(!result.IsSuccess)
                {
                    _noneToday = $"Failed to deserialize: {result.Error}";
                    _logger.Error(_noneToday);
                    return;
                }

                _activeShowInformation = result.Value;
            }
            catch (Exception ex)
            {
                _logger.Error("Encountered an Error: {errMsg}", ex.GetInnerMessage());
            }
        }
    }

    private DateTime ValidDate()
    {
        if (DateValidation.TryCreateSqlDate(Year, Month, Day, out DateTime validDate))
        {
            _dayName = validDate.DayOfWeek;
            return validDate;
        }

        validDate = DateTime.Today;
        _logger.Error($"Invalid date: {Year}-{Month}-{Day}");
        _logger.Information("Using fallback date and time");

        _dayName = validDate.DayOfWeek;
        return validDate;
    }

    private async Task<UserIpDto> GetUserIpDetails()
    {
        UserIpDto userIp = new();
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

    //public async Task LoadDailyShowsFromService(IProgress<int> progress)
    //{
    //    ///*
    //    //* This can be call to anything like calling an api to load employees.
    //    //* During execution 'LoadingTemplate' will be displayed.
    //    //* If your api call returns empty result, then 'EmptyTemplate' will be displayed,
    //    //* this way you have proper feedback, for when your datagrid is loading or empty.
    //    //*/
    //    //progress = 0;
    //    //await InvokeAsync(StateHasChanged);

    //    //await Task.Delay(500);
    //    //progress = 25;
    //    //await InvokeAsync(StateHasChanged);

    //    //await Task.Delay(500);
    //    //progress = 50;
    //    //await InvokeAsync(StateHasChanged);

    //    //await Task.Delay(500);
    //    //progress = 75;
    //    //await InvokeAsync(StateHasChanged);


    //    //await Task.Delay(500);
    //    //progress = 100;
    //    //await InvokeAsync(StateHasChanged);


    //    // Simulate ongoing progress from 0-100%
    //    for (int i = 0; i <= 100; i += 5)
    //    {
    //        await Task.Delay(100);
    //        progress.Report(i);
    //    }

    //    _enabled = false;
    //}

    //public async Task LoadDailyShowsFromService(IList<Task> showTasks, IProgress<int> progress)
    //{
    //    int percentage = 25;
    //    while (showTasks.Count > 0)
    //    {
    //        Task finishedTask = await Task.WhenAny(showTasks);
    //        //showTasks[0].
    //        if (finishedTask == showTasks[0])
    //        {
    //            progress.Report(percentage);
    //            _logger.LogInformation("Shows retrieved");
    //            percentage += 25;
    //        }
    //        else if (finishedTask == showTasks[1])
    //        {
    //            progress.Report(percentage);
    //            _logger.LogInformation("Show information retrieved");
    //            percentage += 25;
    //        }

    //        await finishedTask;
    //        showTasks.Remove(finishedTask);
    //    }

    //    percentage += 25;
    //    progress.Report(percentage);
    //    //Enabled = false;
    //}
}
