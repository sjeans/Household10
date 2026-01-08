using System.Net;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;

namespace Household.SharedComponents.Components.Shared.Forms.Selectors;

public partial class ShowInformationSelector : ComponentBase, IDisposable
{
    [Parameter]
    public string? ShowName { get; set; }

    [Parameter]
    public EventCallback<int> OnSubscriptionUpdate { get; set; }

    [Inject] public required IApiService ApiService { get; set; }
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] public ICacheService<Dictionary<int, string>> Cache { get; set; } = default!;

    public List<SelectOption> Options { get; set; } = [];

    private HttpClient? _client;
    private ILogger _logger = default!;
    private const string ACCESS_TOKEN_KEY = CacheKeys.ShowInformationSelectorKey;
    private Dictionary<int, string> _showNames = [];
    private bool disposedValue;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _logger = Logger.ForContext<ShowInformationSelector>();
        _client = ApiService.HttpClient;
        if (Options.Count == 0)
            await GetAllShowNames();

    }

    protected async Task GetAllShowNames()
    {
        try
        {
            _showNames = await Cache.GetOrCreateAsync(ACCESS_TOKEN_KEY, GetShowNamesAsync, TimeSpan.FromHours(8));

            if (_showNames is null)
            {
                _showNames = await GetShowNamesAsync();
                _logger.Information("Cache miss: Retrieved {count} show names from source.", _showNames?.Count);
            }
            else
                _logger.Information("Retrieved {count} show names from cache.", _showNames?.Count);

            IOrderedEnumerable<KeyValuePair<int, string>> sortedDict = from entry in _showNames orderby entry.Value ascending select entry;

            foreach (KeyValuePair<int, string> item in sortedDict)
            {
                SelectOption itemOption = new()
                {
                    Text = item.Value,
                    Value = item.Key.ToString(),
                    Selected = item.Value == ShowName,
                };

                Options.Add(itemOption);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.GetInnerMessage());
        }
    }

    private async Task<Dictionary<int, string>> GetShowNamesAsync()
    {
        Dictionary<int, string>? showNames = [];
        if (_client == null)
        {
            _logger.Warning("HttpClient is null in GetAllShowNames");
            return showNames;
        }

        try
        {
            HttpResponseMessage response = await _client.GetAsync("api/tvshowinformation/names");
            string responseString = await response.Content.ReadAsStringAsync();
            HttpStatusCode statusCode = response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                showNames = JsonConvert.DeserializeObject<Dictionary<int, string>>(responseString);

                if (showNames is not null)
                {
                    Dictionary<int, string> showNamesTrimmed = await RemoveInactiveShowsAsync(showNames);
                    //IOrderedEnumerable<KeyValuePair<int, string>> sortedDict = from entry in showNamesTrimmed orderby entry.Value ascending select entry;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.GetInnerMessage());
        }
        
        return showNames ?? [];
    }

    private async Task<Dictionary<int, string>> RemoveInactiveShowsAsync(Dictionary<int, string> showNames)
    {
        if (_client == null)
        {
            _logger.Warning("HttpClient is null in RemoveInactiveShowsAsync");
            return showNames;
        }

        HttpResponseMessage response = await _client.GetAsync("api/shows/");
        string responseString = await response.Content.ReadAsStringAsync();
        HttpStatusCode statusCode = response.StatusCode;

        if (response.IsSuccessStatusCode)
        {
            List<TVShowDto>? tvShows = JsonConvert.DeserializeObject<List<TVShowDto>>(responseString);

            if (tvShows is not null)
            {
                List<int> removals = [];
                foreach (KeyValuePair<int, string> activeShow in showNames)
                {
                    TVShowDto? show = tvShows.FirstOrDefault(x => x.Name == activeShow.Value);
                    if (show is not null)
                        if (show.IsCompleted)
                            removals.Add(activeShow.Key);

                }

                if (removals.Count > 0)
                    foreach (int i in removals)
                        showNames.Remove(i);

            }
        }

        return showNames;
    }

    protected Task HandleChange(ChangeEventArgs eventArgs)
    {
        string selectedOption = eventArgs.Value?.ToString() ?? string.Empty;

        if (!selectedOption.IsNullOrWhiteSpace())
        {
            _ = int.TryParse(selectedOption, out int selectedSeason); 

            return OnSubscriptionUpdate.InvokeAsync(selectedSeason);
        }

        //if (int.TryParse(Options.FirstOrDefault(x => x.Selected)?.Value, out int oldState))
        //{
        //    await JsRuntime.InvokeAsync<object>("ResetSelectValue", "ddlSubscriptions", oldState);

        //    return OnSubscriptionUpdate.InvokeAsync(oldState);
        //}

        return Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~ShowInformationSelector()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
