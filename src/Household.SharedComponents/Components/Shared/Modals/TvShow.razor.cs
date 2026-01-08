using System.ComponentModel;
using System.Globalization;
using Blazorise;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Shared.Modals;

public partial class TvShow : ComponentBase
{
    [Parameter]
    public List<TVShowInformationDto>? ShowInformationDto { get; set; } = default!;

    [Parameter]
    public string ShowName { get; set; } = default!;

    [Parameter]
    public string StreamingName { get; set; } = default!;

    [Parameter]
    public int SeasonNumber { get; set; }

    [Parameter]
    public Modal ModalRef { get; set; } = default!;

    public string WhatEpisode { get; set; } = string.Empty;

    private MarkupString _summaryDisplay = default!;

    private int _episodeSeason = 0;
    private int _episodeNumber = 0;
    private int _showId;
    private string _episodeName = string.Empty;
    private string _episodeImgSrc = string.Empty;
    private string _episodeAirTime = string.Empty;
    private string _officialSite = string.Empty;
    private string _episodeRating = string.Empty;
    private string _episodeAirDate = string.Empty;
    private string _episodeRuntime = string.Empty;
    private string _seasonStartDate = string.Empty;
    private string? _showName;
    private string _streamingName = string.Empty;
    private string _missingImg = string.Empty;
    private bool _previousDisable;
    private bool _nextDisable;
    private bool _modalVisible;

    public TvShow()
    {
        ModalRef = new();
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _showName = ShowName;

        int whichMessage = 0;
        TVShowInformationDto? showInfo = ShowInformationDto?.FirstOrDefault(show => show.Name == _showName);
        DateTime? start = new();
        DateTime? end = new();
        List<Episode> currentSeasonEpisodes = [];

        if (whichMessage >= 0)
        {
            int weekNumber = Common.GetIso8601WeekOfYear(DateTime.Today);
            start = Common.FirstDateOfWeek(DateTime.Now.Year, weekNumber, CultureInfo.CurrentCulture);
            end = Common.FirstDateOfWeek(DateTime.Now.Year, weekNumber, CultureInfo.CurrentCulture).AddDays(6);

            Episode? episode = null;
            if (showInfo is not null)
            {
                showInfo.Episodes = showInfo.Episodes?.OrderBy(s => s.Season).ThenBy(n => n.Number).ToList();
                currentSeasonEpisodes = showInfo.Episodes?.Where(f => f.Season == SeasonNumber).ToList() ?? [];
                _seasonStartDate = currentSeasonEpisodes.FirstOrDefault(f => f.Number == 1)?.AirStamp?.ToShortDateString() ?? string.Empty;

                // Latest season
                //_episodeNumber = showInfo.Episodes?.Max(e => e.Number) ?? 1;  // <= this works only if the season count is consistent between seasons
                _episodeNumber = currentSeasonEpisodes.Max(e => e.Number) ?? 1;
                episode = showInfo.Episodes?.LastOrDefault(ep => ep.AirStamp != null && ep.AirStamp.Value >= start && ep.AirStamp.Value <= end);
                episode ??= showInfo.Episodes?.LastOrDefault(ep => ep.Season == SeasonNumber && ep.Number == _episodeNumber);

                if (episode is not null)
                    GetEpidsodeInformation(episode, showInfo!);

                if (whichMessage < episode?.Number)
                    whichMessage = episode.Number.Value;

                if (_episodeNumber == episode?.Number)
                    _nextDisable = true;
                else if (episode?.Number == 1)
                    _previousDisable = true;

                if (_episodeNumber > episode?.Number)
                    _episodeNumber = episode.Number.Value;

            }
        }
    }

    private void GetEpidsodeInformation(Episode episode, TVShowInformationDto showInfo)
    {
        if (episode.AirTime.IsNullOrWhiteSpace())
            episode.AirTime = episode.AirStamp.GetValueOrDefault().ToShortTimeString();

        _showId = showInfo.Id;
        _summaryDisplay = new(episode.Summary ?? "Series summary:</br>" + showInfo.Summary ?? string.Empty);
        _streamingName = StreamingName;

        _episodeName = episode.Name!;
        _episodeSeason = episode.Season ?? 0;
        _episodeImgSrc = episode.Images is not null ? episode.Images!.Medium! : string.Empty;
        _missingImg = _episodeImgSrc.IsNullOrWhiteSpace() ? " placeholder placeholder-lg placeholder-wave episodeImgSrc" : string.Empty;

        _episodeAirTime = DateTime.Parse(episode.AirTime!).ToString(@"h\:mm tt");
        _officialSite = showInfo.OfficialSite ?? string.Empty;

        _episodeRating = episode.Rating?.Average is null ? "No rating" : episode.Rating.Average.Value.ToString();
        _episodeAirDate = Common.FormatDateTime(episode.AirDate);
        _episodeRuntime = (episode.Runtime is null ? showInfo.AverageRuntime.ToString() : episode.Runtime.ToString()) ?? string.Empty;
    }

    private Task GetPreviousOrNext(string previousOrNext)
    {
        if (ShowInformationDto is not null)
        {
            TVShowInformationDto? showInfo = ShowInformationDto?.FirstOrDefault(show => show.Name == _showName);
            Episode? episode = null;

            // Normalize input just once (case-insensitive compare)
            bool isPrevious = string.Equals(previousOrNext, "previous", StringComparison.OrdinalIgnoreCase);
            bool isNext = string.Equals(previousOrNext, "next", StringComparison.OrdinalIgnoreCase);

            if (showInfo is not null)
            {
                if (isPrevious && _episodeNumber > 1)
                {
                    _episodeNumber--;
                    episode = showInfo.Episodes?.LastOrDefault(ep => ep.Number == _episodeNumber);
                }
                else if (isNext && _episodeNumber < showInfo.NumberEpisodes)
                {
                    _episodeNumber++;
                    episode = showInfo.Episodes?.LastOrDefault(ep => ep.Number == _episodeNumber);
                }
            }

            if (episode is not null)
            {
                GetEpidsodeInformation(episode, showInfo!);

                if (isPrevious && _nextDisable) _nextDisable = false;
                if (isNext && _previousDisable) _previousDisable = false;
                if (_episodeNumber == 1) _previousDisable = true;
                if (_episodeNumber == showInfo?.Episodes?.Where(ep => ep.Season == SeasonNumber).Count()) _nextDisable = true;
            }
            else
            {
                // Toggle disable flags
                _previousDisable = isPrevious;
                _nextDisable = isNext;
            }
        }

        return Task.CompletedTask;
    }

    public Task HideModal()
    {
        _modalVisible = false;
        ModalRef.Hide();

        return Task.CompletedTask;
    }

    private Task Previous()
    {
        if (ShowInformationDto is not null)
            GetPreviousOrNext("previous");

        return Task.CompletedTask;
    }

    private Task Next()
    {
        if (ShowInformationDto is not null)
            GetPreviousOrNext("next");

        return Task.CompletedTask;
    }

    private Task OnModalClosing(CancelEventArgs e)
    {
        CloseReason closeReasonEnum = ((ModalClosingEventArgs)e).CloseReason;
        if (closeReasonEnum != CloseReason.UserClosing)
            e.Cancel = true;

        return Task.CompletedTask;
    }

    public Task ShowModal()
    {
        _modalVisible = true;
        ModalRef.Show();

        return Task.CompletedTask;
    }
}
