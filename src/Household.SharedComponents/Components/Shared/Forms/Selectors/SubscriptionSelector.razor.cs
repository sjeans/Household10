using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Serilog;

namespace Household.SharedComponents.Components.Shared.Forms.Selectors;

public partial class SubscriptionSelector : ComponentBase, IDisposable
{
    [Parameter]
    public int SubscriptionId { get; set; }

    [Parameter]
    public EventCallback<int> OnSubscriptionUpdate { get; set; }

    [Parameter]
    public EventCallback<int> OnInValidSelection { get; set; }

    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] public ICacheService<List<StreamingServiceDto>> Cache { get; set; } = default!;

    private List<SelectOption> _options = [];
    private List<StreamingServiceDto> _subscriptions = [];
    private HttpClient? _client;
    private ILogger _logger = default!;
    private const string ACCESS_TOKEN_KEY = CacheKeys.SubscriptionSelectorKey;
    private bool disposedValue;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _logger = Logger.ForContext<SubscriptionSelector>();
        _client = ApiService.HttpClient;
        if(_options.Count == 0)
            await GetAllSubscriptions();
        else
        {
            SelectOption? selectedFound = _options.FirstOrDefault(x => x.Value.Equals(SubscriptionId.ToString()) && x.Selected == true);
            if (selectedFound == null)
            {
                foreach (SelectOption option in _options)
                {
                    if (option.Value.Equals(SubscriptionId.ToString()) && option.Selected == false)
                        option.Selected = true;
                }
            }
        }
    }

    private async Task<List<StreamingServiceDto>> GetAllStreamingSubscriptionsAsync()
    {
        List<StreamingServiceDto> subscriptions = [];
        if (_client == null)
        {
            _logger.Error("HttpClient is not initialized.");
            return subscriptions;
        }
        try
        {
            HttpResponseMessage response = await _client.GetAsync("api/subscriptions");
            response.EnsureSuccessStatusCode();
            await using Stream stream = await response.Content.ReadAsStreamAsync(new());
            Result<List<StreamingServiceDto>> result = await JsonDeserializer.TryDeserializeAsync<List<StreamingServiceDto>>(stream, new());
            if (!result.IsSuccess)
            {
                _logger.Error("Failed to retrieve subscriptions: {Error}", result.Error);
                return subscriptions;
            }
            subscriptions = result.Value ?? [];
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.GetInnerMessage());
            throw;
        }
        return subscriptions;
    }

    protected async Task GetAllSubscriptions()
    {
        if (_client == null)
        {
            _logger.Error("HttpClient is not initialized.");
            return;
        }

        try
        {
            _subscriptions = await Cache.GetOrCreateAsync(ACCESS_TOKEN_KEY, GetAllStreamingSubscriptionsAsync, TimeSpan.FromHours(8));

            if (_subscriptions is null)
            {
                _subscriptions = await GetAllStreamingSubscriptionsAsync();
                _logger.Information("Cache miss: Retrieved {count} subscriptions from source.", _subscriptions?.Count);
            }
            else
                _logger.Information("Retrieved {count} subscriptions from cache.", _subscriptions?.Count);

            if (_subscriptions is not null && _subscriptions.Count > 0)
            {
                _subscriptions.ForEach(ss =>
                {
                    SelectOption itemOption = new()
                    {
                        Text = ss.Name,
                        Value = ss.Id.ToString(),
                        Selected = ss.Id == SubscriptionId,
                    };

                    _options.Add(itemOption);
                });
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.GetInnerMessage());
            throw;
        }
    }

    protected async Task<Task> HandleChange(ChangeEventArgs eventArgs)
    {
        string selectedOption = eventArgs.Value?.ToString() ?? string.Empty;

        if (!selectedOption.IsNullOrWhiteSpace())
        {
            _ = int.TryParse(selectedOption, out int selectedSeason);

            return OnSubscriptionUpdate.InvokeAsync(selectedSeason);
        }

        if (int.TryParse(_options.FirstOrDefault(x => x.Selected)?.Value, out int oldState))
        {
            await JsRuntime.InvokeAsync<object>("ResetSelectValue", "ddlSubscriptions", oldState);

            return OnSubscriptionUpdate.InvokeAsync(oldState);
        }

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
    ~SubscriptionSelector()
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
