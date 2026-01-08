using System.Net.NetworkInformation;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Serilog;

namespace Household.SharedComponents.Components.Shared.Forms.Selectors;

public partial class EpisodeInformationSelector : ComponentBase, IDisposable
{
    private bool disposedValue;

    [Parameter]
    public List<Episode>? EpisodeList { get; set; }

    [Parameter]
    public string EpisodeName { get; set; } = default!;

    [Parameter]
    public EventCallback<int> OnEpisodeDetailUpdate { get; set; }

    [Inject] public required IApiService ApiService { get; set; }
    [Inject] private ILogger Logger { get; set; } = default!;

    private ILogger _logger = default!;

    public List<SelectOption> Options { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        // incase we need to initialize something
        _logger = Logger.ForContext<EpisodeInformationSelector>();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Options.Count == 0)
            await GetAllEpisodeNames();

    }

    protected Task GetAllEpisodeNames()
    {
        if (EpisodeList == null)
        {
            _logger.Warning("EpisodeList is null, cannot populate episode names.");
            return Task.CompletedTask;
        }

        try
        {
            /// order doesn't completely work
            //EpisodeList.AsParallel().OrderBy(item => item.TvMazeId).ForAll(episode =>
            //{
            //    SelectOption option = new()
            //    {
            //        Text = episode.Name,
            //        Value = episode.TvMazeId.ToString(),
            //        Selected = episode.Name == EpisodeName,
            //    };

            //    Options.Add(option);
            //});

            foreach (Episode episode in EpisodeList)
            {
                SelectOption option = new()
                {
                    Text = episode.Name,
                    Value = episode.TvMazeId.ToString(),
                    Selected = episode.Name == EpisodeName,
                };

                Options.Add(option);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.GetInnerMessage());
            throw;
        }

        return Task.CompletedTask;
    }

    protected Task HandleChange(ChangeEventArgs eventArgs)
    {
        string selectedOption = eventArgs.Value?.ToString() ?? string.Empty;

        if (!selectedOption.IsNullOrWhiteSpace())
        {
            _ = int.TryParse(selectedOption, out int selectedEpisode);

            return OnEpisodeDetailUpdate.InvokeAsync(selectedEpisode);
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
    ~EpisodeInformationSelector()
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
