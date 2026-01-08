using Blazorise;
using Household.Shared.Dtos;
using Household.Shared.Enums;
using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Household.SharedComponents.Components.Shared.Modals;
using Serilog;

namespace Household.SharedComponents.Components.Shared.Forms;

public partial class AddEditTvShow : ComponentBase, IDisposable
{
    [CascadingParameter]
    public EditContext EditContext { get; set; } = default!;

    [Parameter]
    public required int TvShowId { get; set; }

    [Parameter]
    public required TVShowDto TVShow { get; set; }

    [Parameter]
    public bool CanSave { get; set; }

    [Parameter]
    public string CanShow { get; set; } = default!;

    [Parameter]
    public string ClientId { get; set; } = default!;

    [Parameter]
    public HttpClient? Client { get; set; }

    // Optional: parent can pass its own logger
    //[Parameter] public ILogger<Edit>? Logger { get; set; }
    [Parameter] public ILogger? Logger { get; set; }

    [Parameter]
    public Notification ComponentNotification { get; set; } = default!;

    [Parameter]
    public string Message { get; set; } = default!;

    [Inject] private ILogger DefaultLogger { get; set; } = default!;

    // Pick whichever is available
    private ILogger _activeLogger => Logger ?? DefaultLogger;
    private HttpClient? _client = default!;

    private string _showName = string.Empty;
    private string _canShow = string.Empty;
    private string _clientId = string.Empty;
    private bool _canSave = false;
    private int _selectedValue;
    private decimal _selectedDecimalValue;
    private bool disposedValue;

    protected override void OnParametersSet() // OnInitialized()
    {
        _activeLogger.Information("Parameters setup for add/editing tv show: {Id}", TvShowId);

        _client = Client;
        _canShow = CanShow;
        _clientId = ClientId;
        _canSave = CanSave;

        if (TvShowId > 0)
        {
            GetShowDetails();

            if (!_showName.IsNullOrWhiteSpace())
                _activeLogger.Information("Ready to edit the show {name}'s details.", _showName);

        }
        else
            _activeLogger.Information("Ready to add TV show details.");

    }

    private void GetShowDetails()
    {
        _activeLogger.Information("Setting up show details.");
        TVShowDto? tvShow = TVShow;

        if (tvShow != null)
        {
            _showName = tvShow.Name;
            _selectedValue = (int)tvShow.Rating;
            _selectedDecimalValue = tvShow.Rating;
        }
    }

    private RatingTooltip? GetTooltip(decimal value)
    {
        if (value <= 2)
            return new RatingTooltip("Very bad");
        else if (value <= 4)
            return new RatingTooltip("Bad", TooltipPlacement.Bottom);
        else if (value <= 6)
            return new RatingTooltip("Fair");
        else if (value <= 8)
            return new RatingTooltip("Good", TooltipPlacement.Top, false, false);
        else if (value <= 10)
            return new RatingTooltip("Very good");

        return null;
    }

    protected static Dictionary<string, object> HandyFunction()
    {
        Dictionary<string, object> dict = new()
        {
            { "autocomplete", true }
        };
        return dict;
    }

    protected void EpisodeUpdate(int newEpisode)
    {
        TVShow.Episodes = newEpisode;
        EditContext.NotifyFieldChanged(EditContext.Field("Episodes"));
    }

    protected void SeasonUpdate(int newSeason)
    {
        TVShow.Season = (Seasons)newSeason;
        EditContext.NotifyFieldChanged(EditContext.Field("Season"));
    }

    protected void SubscriptionUpdate(int newSubscription)
    {
        TVShow.StreamingId = newSubscription;
        EditContext.NotifyFieldChanged(EditContext.Field("StreamingId"));
    }

    protected void WeekDayUpdate(DayOfWeek newWeekDay)
    {
        TVShow.DayOfWeek = newWeekDay;
        EditContext.NotifyFieldChanged(EditContext.Field("DayOfWeek"));
        //EditContext.NotifyFieldChanged(FieldIdentifier.Create(() => TVShow.DayOfWeek));
    }

    protected static void ValidateProperty(ValidatorEventArgs eventArgs)
    {
        bool selection = int.TryParse((string?)eventArgs.Value, out _);
        eventArgs.Status = selection ? ValidationStatus.Success : ValidationStatus.Error;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                if (EditContext is not null)
                {
                    EditContext.OnFieldChanged -= null;
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~AddEditTvShow()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    void IDisposable.Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
