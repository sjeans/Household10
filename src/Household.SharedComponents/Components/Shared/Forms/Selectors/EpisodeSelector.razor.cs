using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Household.SharedComponents.Components.Shared.Forms.Selectors;

public partial class EpisodeSelector : ComponentBase, IDisposable
{
    [Parameter]
    public int EpisodeCount { get; set; }

    [Parameter]
    public string DefaultOption { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<int> OnEpisodeUpdate { get; set; }

    [Inject] IJSRuntime JsRuntime { get; set; } = default!;

    private List<SelectOption> _options = [];

    private const string _defaultOption = "Choose how many Episodes";
    private bool disposedValue;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        int maxNumEpisodes = Math.Max(24, EpisodeCount);
        int minNumEpisodes = 1;

        if (_options.Count == 0)
        {
            // Build once
            _options = Enumerable.Range(1, maxNumEpisodes - minNumEpisodes + 1)
                                 .Select(i => new SelectOption
                                 {
                                     Text = i.ToString(),
                                     Value = i.ToString(),
                                     Selected = (i == EpisodeCount)
                                 }).ToList();

            // Mark selected (if valid)
            if (EpisodeCount > 0)
                await OnEpisodeUpdate.InvokeAsync(EpisodeCount);

        }
    }

    protected async Task<Task> HandleChange(ChangeEventArgs eventArgs)
    {
        string selectedOption = eventArgs.Value?.ToString() ?? string.Empty;

        if (!selectedOption.IsNullOrWhiteSpace())
        {
            int.TryParse(selectedOption, out int selectedSeason);

            return OnEpisodeUpdate.InvokeAsync(selectedSeason);
        }

        if (int.TryParse(_options.FirstOrDefault(x => x.Selected)?.Value, out int oldState))
        {
            await JsRuntime.InvokeAsync<object>("ResetSelectValue", "ddlNumberOfEpisodes", oldState);

            return OnEpisodeUpdate.InvokeAsync(oldState);
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
    ~EpisodeSelector()
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
