using System.Net.Http.Json;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Household.SharedComponents.Components.Shared.Modals;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Serilog;
using Alert = Household.SharedComponents.Components.Shared.Messages;

namespace Household.SharedComponents.Components.Pages.StreamingServices;

public partial class Edit
{
    [Parameter]
    public int Id { get; set; }

    private bool IsEnabled { get; set; } = true;

    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    [Inject] private AuthenticationStateProvider Auth { get; set; } = default!;
    [Inject] private ICacheService<List<StreamingServiceDto>> Cache { get; set; } = default!;

    private ICacheService<List<StreamingServiceDto>> _cache = default!;
    private const string ACCESS_TOKEN_KEY = CacheKeys.StreamingServicesKey;

    private string _streamingName = string.Empty;
    private StreamingServiceDto _streamingService = new();
    private string? _message = string.Empty;

    private bool _canSave;
    private string _canShow = default!;

    private Loading _loadingIndicator = default!;

    private UserIpDto _userIp = default!;
    private StreamingServiceDto? _originalService;
    internal Notification _notification = new();
    internal Alert.Notification _alertNotification = new();
    private HttpClient? _client;
    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _logger = Logger.ForContext<Edit>();
        _logger.Information("Initializing edit streaming service.");
        _client = ApiService.HttpClient;
        AuthenticationState st = await Auth.GetAuthenticationStateAsync();
        _cache = Cache;

        bool isAuthenticated = st.User.Identity?.IsAuthenticated ?? false;

        if (isAuthenticated)
        {
            if (Id > 0)
            {
                _userIp = await GetUserIpDetails();
                _canSave = _userIp.CanSave;
                _canShow = _userIp.CanShow;
                _logger.Information("{msg}", _userIp.LogMessage);

                await _loadingIndicator.ShowAsync();
                await GetStreamingServiceDetails();
                PageHistoryState.AddPageToHistory("/streamingservices/index");
                await _loadingIndicator.HideAsync();
            }
        }
    }

    private async Task GetStreamingServiceDetails()
    {
        if (_client == null)
        {
            _message = "Cannot make client calls for data!";
            _logger.Error(_message);
            return;
        }

        try
        {
            _logger.Information("Retrieving streaming service to edit.");
            HttpResponseMessage response = await _client.GetAsync($"api/subscriptions/{Id}");

            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync(new());
            Result<StreamingServiceDto> result = await JsonDeserializer.TryDeserializeAsync<StreamingServiceDto>(stream, new());

            if(!result.IsSuccess)
            {
                _message = $"Failed to deserialize: {result.Error}";
                _logger.Error(_message);
                return;
            }

            StreamingServiceDto? streamingService = result.Value;

            if (streamingService != null)
            {
                _originalService = new()
                {
                    Amount = streamingService.Amount,
                    Description = streamingService.Description,
                    Id = streamingService.Id,
                    Name = streamingService.Name,
                    PaySchedule = streamingService.PaySchedule,
                    Subscription = streamingService.Subscription,
                    StartDate = streamingService.StartDate,
                };
                _streamingService = streamingService;
                _streamingName = streamingService.Name;
            }
            else
                _message = "Streaming services not found...";

        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered an error updating streaming service.{newLine}Error: {errMsg}", Environment.NewLine, ex.GetInnerMessage());
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

    protected async Task HandleSubmit(EditContext editContext)
    {
        if (_client == null)
        {
            _logger.Error("Cannot make client calls to update data!");
            return;
        }

        if (!editContext.Validate())
        {
            //string keysString = string.Join(", ", data.FirstOrDefault()?.Keys ?? Enumerable.Empty<string>());
            _message = string.Join($"{Environment.NewLine}", editContext.GetValidationMessages());
            _alertNotification?.Show(2, true, _message);
            _logger.Error("Validation error encountered: {erMsg}", _message);
            return;
        }

        if (editContext.IsModified())
        {
            StreamingServiceDto updatedService = (StreamingServiceDto)editContext.Model;
            bool isJsonEqual = updatedService.JsonCompare(_originalService!);

            if (isJsonEqual) // no changes
            {
                _message = "No changes found!";
                _alertNotification?.Show(2, true, _message);
            }
            else
            {
                // changes
                _logger.Information("{ip} is updating streaming service.", _userIp.IpAddress);
                HttpResponseMessage response = await _client.PutAsJsonAsync("api/subscriptions/updateservice", updatedService);

                _message = response.StatusCode.ToString();
                _logger.Information("Update from {ip} streaming service success: {success}", _userIp.IpAddress, _message);
                if (response.IsSuccessStatusCode)
                {
                    _notification?.Show(1, true, "You have successfully updated the service!");
                    _logger.Information("Expired cache {success}", await _cache.ExpireAsync(ACCESS_TOKEN_KEY));
                }
            }
        }
        else
        {
            _message = "No changes found!";
            _alertNotification?.Show(2, true, _message);
        }
    }
}
