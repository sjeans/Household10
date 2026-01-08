using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Household.SharedComponents.Components.Shared.Forms.Selectors;

public partial class DayOfWeekSelector : ComponentBase, IDisposable
{
    [Parameter]
    public DayOfWeek WeekDay { get; set; }

    [Parameter]
    public EventCallback<DayOfWeek> OnWeekDayUpdate { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private readonly List<SelectOption> _dayOfWeek = [];
    private bool disposedValue;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        foreach (object dayofweek in Enum.GetValues(typeof(DayOfWeek)))
        {
            DayOfWeek day = (DayOfWeek)dayofweek;
            string dayValue = Convert.ToInt32(day).ToString();

            _dayOfWeek.Add(new SelectOption
            {
                Text = day.ToString(),
                Value = dayValue,
                Selected = day == WeekDay
            });
        }
    }

    protected async Task<Task> HandleChange(ChangeEventArgs eventArgs)
    {
        string selectedOption = eventArgs.Value?.ToString() ?? string.Empty;

        if (!selectedOption.IsNullOrWhiteSpace())
        {
            _ = int.TryParse(selectedOption, out int dayOfWeek);

            return OnWeekDayUpdate.InvokeAsync((DayOfWeek)dayOfWeek);
        }

        if (int.TryParse(_dayOfWeek.FirstOrDefault(x => x.Selected)?.Value, out int oldDay))
        {
            await JsRuntime.InvokeVoidAsync("SelectReset", "ddlDayOfWeek", oldDay);

            return OnWeekDayUpdate.InvokeAsync((DayOfWeek)oldDay);
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
    ~DayOfWeekSelector()
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
