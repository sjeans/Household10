using Household.Shared.Dtos;
using Household.Shared.Enums;
using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Household.SharedComponents.Components.Shared.Forms.Selectors;

public partial class StatesSelector : ComponentBase, IDisposable
{
    private bool disposedValue;

    [Parameter]
    public string? SelectedState { get; set; }

    [Parameter]
    public EventCallback<int> OnStatesUpdate { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    public List<SelectOption> States { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (States.Count == 0)
        {
            Array statesArray = Enum.GetValues(typeof(States));

            foreach (object state in statesArray)
            {
                States theState = (States)state;
                string stateIntValue = Convert.ToInt32(theState).ToString();
                string stateNameValue = theState.ToString();

                States.Add(new SelectOption
                {
                    Text = stateNameValue ?? string.Empty,
                    Value = stateIntValue,
                    Selected = theState.ToString() == SelectedState
                });
            }
        }
    }

    protected async Task<Task> HandleChange(ChangeEventArgs eventArgs)
    {
        string selectedOption = eventArgs.Value?.ToString() ?? string.Empty;

        if (!selectedOption.IsNullOrWhiteSpace())
        {
            _ = int.TryParse(selectedOption, out int selectedState);

            return OnStatesUpdate.InvokeAsync(selectedState);
        }

        if (int.TryParse(States.FirstOrDefault(x => x.Selected)?.Value, out int oldState))
        {
            await JsRuntime.InvokeAsync<object>("ResetSelectValue", "ddlStates", oldState);

            return OnStatesUpdate.InvokeAsync(oldState);
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
    ~StatesSelector()
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
