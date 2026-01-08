using System.Globalization;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Shared;

public partial class TvShowItem : ComponentBase
{
    [Parameter]
    public TVShowDto? TvShow { get; set; }

    [Parameter]
    public string DisableLink { get; set; } = string.Empty;

    [Parameter]
    public List<TVShowInformationDto>? ShowInformationDto { get; set; } = default!;

    [Inject] ITvScheduleService TvScheduleService { get; set; } = default!;

    private string _seasonStartDate = string.Empty;
    private string _whatEpisode = string.Empty;
    private Modals.TvShow _modalRef = default!;
    private string? _showName;
    private int _seasonNumber;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        int whichMessage = 0;
        if (TvShow != null)
        {
            whichMessage = DateTime.TryParse(TvShow.StartDate, out DateTime date) ? Common.GetNextEpisode(TvShow.Episodes, date, !TvShow.IsCompletedSeason, TvShow.IsCompleted) : -1;
            _showName = TvShow.Name;

            TVShowInformationDto? showInfo = ShowInformationDto?.FirstOrDefault(show => show.Name == _showName);
            DateTime? start = new();
            DateTime? end = new();

            if (showInfo is not null)
            {
                showInfo.Episodes = showInfo.Episodes?.OrderBy(s => s.Season).ThenBy(n => n.Number).ToList();
                Episode? firstEpisode = showInfo.Episodes?.FirstOrDefault(f => f.Season == (int)TvShow.Season && f.Number == 1);

                // Latest season
                _seasonNumber = showInfo.Episodes?.Max(e => e.Season) ?? 1;
                _seasonStartDate = firstEpisode?.AirStamp?.ToShortDateString() ?? string.Empty;

                Episode? episode = null;
                int weekNumber = Common.GetIso8601WeekOfYear(DateTime.Today);
                start = Common.FirstDateOfWeek(DateTime.Now.Year, weekNumber, CultureInfo.CurrentCulture);
                end = Common.FirstDateOfWeek(DateTime.Now.Year, weekNumber, CultureInfo.CurrentCulture).AddDays(6);

                episode = showInfo.Episodes?.LastOrDefault(ep => ep.AirStamp != null && ep.AirStamp.Value >= start && ep.AirStamp.Value <= end);
                if (whichMessage >= 0)
                {
                    if (whichMessage < episode?.Number)
                        whichMessage = episode.Number.Value;

                    if (episode?.Number < whichMessage)
                        whichMessage = episode?.Number.Value ?? 0;

                    _whatEpisode = TvScheduleService.GetSeriesStatus(showInfo, DateTime.Today.Date, whichMessage);
                }
            }
        }
    }

    private Task ShowModal()
    {
        _modalRef.ShowModal();
        return Task.CompletedTask;
    }
}
