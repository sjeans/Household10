using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Serilog;
using Household.SharedComponents.Components.Shared.Modals;
using Alert = Household.SharedComponents.Components.Shared.Messages;

namespace Household.SharedComponents.Components.Pages.DailyShows;

public partial class Add : ComponentBase
{
    [Inject] private IPageHistoryState PageHistoryState { get; set; } = default!;
    [Inject] public required IApiService ApiService { get; set; }
    [Inject] public ILogger Logger { get; private set; } = default!;

    private TVShowDto _tVShow = default!;
    private string _message = string.Empty;

    private UserIpDto _userIp = default!;
    private readonly TVShowDto _originalTvShow = default!;
    private Notification _notification = new();
    private Alert.Notification _alertNotification = new();
    private HttpClient? _client;
    private bool _editContextInitialized = false;

    private bool IsEnabled { get; set; } = false;

    public required EditContext EditContextRef { get; set; }

    private ILogger _logger = default!;

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<Add>();
        _logger.Information("Initializing add show.");
        PageHistoryState.AddPageToHistory("/dailyshows");
        _tVShow = new();
        _client = ApiService.HttpClient;

        if (EditContextRef is null)
        {
            EditContextRef = new EditContext(_tVShow);
        }

        if (!_editContextInitialized && EditContextRef is not null)
        {
            EditContextRef.OnFieldChanged += EditContext_OnFieldChanged;
            _editContextInitialized = true;
        }
        //EditContextRef.OnFieldChanged += EditContext_OnFieldChanged;
        ValidationContext validationContext = new(_tVShow);

        _userIp = await GetUserIpDetails();
        _logger.Information("{msg}", _userIp.LogMessage);
    }

    protected static Dictionary<string, object> HandyFunction()
    {
        Dictionary<string, object> dict = new()
        {
            { "autocomplete", true }
        };
        return dict;
    }

    private string GetValidationCssClass(string fieldName)
    {
        FieldIdentifier fieldIdentifier = EditContextRef.Field(fieldName);
        if (EditContextRef.IsModified(fieldIdentifier))
        {
            return EditContextRef.IsValid(fieldIdentifier) ? "is-valid" : "is-invalid";
        }
        return "";
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

    private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        _logger.Information("The following {type} field {fieldName} was updated by {ip}", e.FieldIdentifier.Model.GetType().Name, e.FieldIdentifier.FieldName, _userIp.IpAddress);
    }

    protected async Task HandleSubmit(EditContext editContext)
    {
        if (_client == null)
        {
            _message = "Cannot make client call to retrieve data!";
            //_alertNotification.Show(3, true, _message);
            return;
        }

        if (!editContext.Validate())
        {
            _message = "No changes found!";
            return;
        }

        if (editContext.IsModified())
        {
            TVShowDto updatedShow = (TVShowDto)editContext.Model;
            bool isJsonEqual = updatedShow.JsonCompare(_originalTvShow);

            if (isJsonEqual) // no changes
            {
                _message = "No changes found! Please make some changes before saving.";
                //_alertNotification.Show(2, true, _message);
            }
            else
            {
                // changes
                _logger.Information("{ip} is adding new show.", _userIp.IpAddress);
                HttpResponseMessage response = await _client.PostAsJsonAsync("api/shows/", updatedShow);

                _message = response.StatusCode.ToString();
                _logger.Information("Add from {ip} was success: {success}", _userIp.IpAddress, _message);

                if (response.IsSuccessStatusCode)
                {
                    //_notification.Show(1, true, "You have successfully added the show!");
                }
            }
        }
        else
        {
            _message = "No changes found!";
            //_alertNotification.Show(2, true, _message);
        }
    }
}
