using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Loader;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Pages.StreamingServices;

public partial class Index
{
    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;
    [Inject] private ICacheService<List<StreamingServiceDto>> Cache { get; set; } = default!;

    private ICacheService<List<StreamingServiceDto>> _cache = default!;
    private const string ACCESS_TOKEN_KEY = CacheKeys.StreamingServicesKey;

    private List<StreamingServiceDto>? _serviceDto = default!;
    private string? _message = default;
    private bool _enabled;
    private bool _showInputs;

    private string _disable = default!;
    private string _visible = default!;

    private Loading _loadingIndicator = default!;

    private UserIpDto _userIp = default!;
    private HttpClient? _client;
    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _logger = Logger.ForContext<Index>();
        _logger.Warning("Initializing streaming services index");

        _client = ApiService.HttpClient;
        _cache = Cache;
        _enabled = true;
        if (_serviceDto == null || _serviceDto.Count == 0)
        {
            _userIp = await GetUserIpDetails();

            await _loadingIndicator.ShowAsync();
            _showInputs = _loadingIndicator.IsVisible;
            await GetStreamingServicesAsync();

            _visible = _userIp.Visible;
            _disable = _userIp.DisableButton;
            _logger.Information("{msg}", _userIp.LogMessage);
            PageHistoryState.AddPageToHistory("/streamingservices/index");
            await _loadingIndicator.HideAsync();
            _showInputs = _loadingIndicator.IsVisible;
        }
    }

    private async Task<List<StreamingServiceDto>> GetAllStreamingServicesAsync()
    {
        if (_client == null)
        {
            _message = "Cannot make client calls for data!";
            _logger.Error(_message);
            return [];
        }

        try
        {
            _logger.Warning("{ip} is retrieving services", _userIp.IpAddress);
            HttpResponseMessage response = await _client.GetAsync("api/subscriptions/");

            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync(new());
            Result<List<StreamingServiceDto>> result = await JsonDeserializer.TryDeserializeAsync<List<StreamingServiceDto>>(stream, new());

            if (!result.IsSuccess)
            {
                _message = $"Failed to deserialize: {result.Error}";
                _logger.Error(_message);
                return [];
            }

            _logger.Warning("{ip} retrieved {count} services", _userIp.IpAddress, result!.Value!.Count);

            return result!.Value!;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Encountered an error: {errMsg}", ex.GetInnerMessage());
            _message = $"Encountered an error: {ex.GetInnerMessage()}";
        }

        return [];
    }

    private async Task GetStreamingServicesAsync()
    {
        _serviceDto = await _cache.GetOrCreateAsync(ACCESS_TOKEN_KEY, GetAllStreamingServicesAsync, TimeSpan.FromHours(3));

        if (_serviceDto is null)
        {
            _serviceDto = await GetAllStreamingServicesAsync();
            _logger.Information("Cache miss: Retrieved {count} shows from source.", _serviceDto?.Count);
        }
        else
            _logger.Information("Retrieved {count} shows from cache.", _serviceDto?.Count);

        _enabled = false;
    }

    protected static string GetSubscription(string? subscription)
    {
        return subscription.IsNullOrWhiteSpace() ? string.Empty : subscription;
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
