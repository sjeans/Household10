using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Dto = Household.Shared.Dtos;
using TvModal = Household.SharedComponents.Components.Shared.Modals;

namespace Household.SharedComponents.Components.Shared;

public partial class TvShowItem_CompletedSeries
{
    [Parameter]
    public Dto.TVShowDto? TvShow { get; set; }

    [Parameter]
    public string DisableLink { get; set; } = string.Empty;

    [Parameter]
    public List<Dto.TVShowInformationDto>? ShowInformationDto { get; set; } = default!;

    private string _whatEpisode = string.Empty;
    private TvModal.TvShow _modalRef = default!;
    private string? showName;
    private string? _streamingName;
    private int _seasonNumber;
    private int _streamingId;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _modalRef = new();

        int whichMessage = 0;
        if (TvShow != null)
        {
            whichMessage = DateTime.TryParse(TvShow.StartDate, out DateTime date) ? Common.GetNextEpisode(TvShow.Episodes, date, !TvShow.IsCompletedSeason, TvShow.IsCompleted) : -1;
            showName = TvShow.Name;
            _streamingName = TvShow.StreamingName;
            _streamingId = TvShow.StreamingId;

            Dto.TVShowInformationDto? showInfo = ShowInformationDto?.FirstOrDefault(show => show.Name == showName);

            if (showInfo is not null)
            {
                // Latest season
                _seasonNumber = showInfo.Episodes?.Max(e => e.Season) ?? 1;
            }
        }
    }

    private Task ShowModal()
    {
        return _modalRef.ShowModal();
    }
}
