using Household.Shared.Enums;
using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Household.SharedComponents.Components.Shared.Forms.Selectors;

public partial class SeasonsSelector : ComponentBase, IDisposable
{
    private bool disposedValue;

    [Parameter]
    public Seasons Season { get; set; }

    [Parameter]
    public EventCallback<int> OnSeasonUpdate { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    protected override void OnInitialized() // OnParametersSet()
    {
        base.OnInitialized();
        //
    }

    protected async Task<Task> HandleChange(ChangeEventArgs eventArgs)
    {
        string selectedOption = eventArgs.Value?.ToString() ?? string.Empty;

        if (!selectedOption.IsNullOrWhiteSpace())
        {
            int.TryParse(selectedOption, out int selectedSeason);

            return OnSeasonUpdate.InvokeAsync(selectedSeason);
        }

        int numericValue = (int)Season;
        if (numericValue > -1)
        {
            await JsRuntime.InvokeAsync<object>("ResetSelectValue", "ddlSeasons", numericValue);

            return OnSeasonUpdate.InvokeAsync(numericValue);
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
    ~SeasonsSelector()
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
