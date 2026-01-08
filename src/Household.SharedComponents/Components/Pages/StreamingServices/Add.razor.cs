using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Household.SharedComponents.Components.Shared.Modals;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Serilog;
using Alert = Household.SharedComponents.Components.Shared.Messages;

namespace Household.SharedComponents.Components.Pages.StreamingServices;

public partial class Add
{
    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private ICacheService<List<StreamingServiceDto>> Cache { get; set; } = default!;

    private ICacheService<List<StreamingServiceDto>> _cache = default!;
    private const string ACCESS_TOKEN_KEY = CacheKeys.StreamingServicesKey;

    private bool IsEnabled { get; set; } = false;
    private EditContext EditContextRef { get; set; } = default!;

    private StreamingServiceDto _streamingService = default!;
    private string _message = string.Empty;

    private UserIpDto _userIp = default!;
    private bool _canSave = default!;
    private string _canShow = default!;
    private readonly StreamingServiceDto? _originalService = new();
    private Notification? _notification;
    private Alert.Notification? _alertNotification;
    private HttpClient? _client;
    private ILogger _logger = default!;
    private bool _editContextInitialized = false;

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<Add>();
        _logger.Information("Initializing add streaming service.");
        PageHistoryState.AddPageToHistory("/streamingservices/index");
        _cache = Cache;
        _streamingService = new();
        _client = ApiService.HttpClient;

        EditContextRef ??= new EditContext(_streamingService);

        if (!_editContextInitialized && EditContextRef is not null)
        {
            EditContextRef.OnFieldChanged += EditContext_OnFieldChanged;
            _editContextInitialized = true;
        }

        ValidationContext validationContext = new(_streamingService);

        _userIp = await GetUserIpDetails(); // <- the web call is bad

        _canSave = _userIp.CanSave;
        _canShow = _userIp.CanShow;
        _logger.Information("{msg}", _userIp.LogMessage);
    }

    private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        _logger.Information("The following {type} field {fieldName} was updated by {ip}", e.FieldIdentifier.Model.GetType().Name, e.FieldIdentifier.FieldName, _userIp.IpAddress);
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
            _logger.Error("Cannot make client calls to add data!");
            return;
        }

        if (!editContext.Validate())
        {
            _logger.Warning("Validation errors occurred. Submission aborted.");
            return;
        }

        if (editContext.IsModified())
        {
            StreamingServiceDto? updatedService = editContext.Model as StreamingServiceDto;
            bool isJsonEqual = updatedService?.JsonCompare(_originalService!) ?? false;

            if (isJsonEqual) // no changes
            {
                _message = "No changes found! Please make some changes before submitting.";
                _alertNotification?.Show(2, true, _message);
            }
            else
            {
                // changes
                _logger.Information("{ip} is adding a streaming service.", _userIp.IpAddress);
                HttpResponseMessage response = await _client.PostAsJsonAsync("api/subscriptions/", updatedService);

                _message = response.StatusCode.ToString();
                _logger.Information("Add from {ip} was success: {success}.", _userIp.IpAddress, _message);
                if (response.IsSuccessStatusCode)
                {
                    _notification?.Show(1, true, "You have successfully added the service!");
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
