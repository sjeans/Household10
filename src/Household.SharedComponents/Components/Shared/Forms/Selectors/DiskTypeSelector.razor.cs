using Household.Shared.Dtos;
using Household.Shared.Enums;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Serilog;

namespace Household.SharedComponents.Components.Shared.Forms.Selectors;

public partial class DiskTypeSelector : ComponentBase, IDisposable
{
    [Parameter]
    public DvdTypes? DvdSelectedType { get; set; }

    [Parameter]
    public EventCallback<int> OnDiskTypeUpdate { get; set; }

    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;
    [Inject] public ICacheService<List<Dvdtype>> Cache { get; set; } = default!;

    private const string ACCESS_TOKEN_KEY = CacheKeys.DvdTypesKey;
    private List<Dvdtype> _dvdTypes = default!;
    private readonly List<SelectOption> _options = [];
    private HttpClient? _client;
    private ILogger _logger = default!;
    private bool disposedValue;

    protected override async Task OnInitializedAsync()
    {
        _logger = Logger.ForContext<DiskTypeSelector>();
        _client = ApiService.HttpClient;
        await GetAllDiskTypes();
    }

    private async Task GetAllDiskTypes()
    {
        try
        {
            _dvdTypes = await Cache.GetOrCreateAsync(ACCESS_TOKEN_KEY, GetDiskTypes, TimeSpan.FromHours(8));

            if (_dvdTypes is null)
            {
                _dvdTypes = await GetDiskTypes();
                _logger.Information("Cache miss: Retrieved {count} dvd types from source.", _dvdTypes?.Count);
            }
            else
                _logger.Information("Retrieved {count} dvd types from cache.", _dvdTypes?.Count);

            _dvdTypes?.ForEach(ss =>
            {
                SelectOption itemOption = new()
                {
                    Text = ss.Name,
                    Value = ss.Id.ToString(),
                    Selected = ss.Id == (int)DvdSelectedType!,
                };

                _options.Add(itemOption);
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.Message);
        }
    }

    private async Task<List<Dvdtype>> GetDiskTypes()
    {
        List<Dvdtype>? dvdTypes = [];

        if (_client == null)
        {
            _logger.Error("HttpClient is not initialized.");
            return dvdTypes;
        }

        HttpResponseMessage response = await _client.GetAsync("api/dvdtype");

        response.EnsureSuccessStatusCode();

        await using Stream stream = response.Content.ReadAsStream(new());
        Result<List<Dvdtype>> result = await JsonDeserializer.TryDeserializeAsync<List<Dvdtype>>(stream, new());

        if (!result.IsSuccess)
        {
            _logger.Error("Failed to deserialize DVD types: {error}", result.Error);
            return dvdTypes;
        }

        dvdTypes = result.Value;

        return dvdTypes ?? [];
    }

    protected Task HandleChange(ChangeEventArgs eventArgs)
    {
        return OnDiskTypeUpdate.InvokeAsync(Convert.ToInt32(eventArgs.Value));
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
    ~DiskTypeSelector()
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
