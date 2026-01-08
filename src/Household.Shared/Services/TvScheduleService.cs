using System.Text;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;

namespace Household.Shared.Services;

public class TvScheduleService : ITvScheduleService
{
    public string GetSeriesStatus(TVShowInformationDto showInfo, DateTime? todayOverride, int whichMessage)
    {
        if (showInfo?.Episodes == null || !showInfo.Episodes.Any())
            return string.Empty;

        DateTime today = (todayOverride ?? DateTime.Today).Date;

        // Latest season
        int latestSeason = showInfo.Episodes.Max(e => e.Season) ?? 1;

        // Episodes in latest season with valid dates
        List<Models.Episode> seasonEpisodes = showInfo.Episodes
            .Where(e => e.Season == latestSeason && e.AirStamp.HasValue)
            .OrderBy(e => e.Number)
            .ToList();

        if (!seasonEpisodes.Any())
            return string.Empty;

        DateTime seasonStart = GetDate(seasonEpisodes.First().AirStamp);
        DateTime seasonEnd = GetDate(seasonEpisodes.Last().AirStamp);
        int totalEpisodes = seasonEpisodes.Count;

        string msgGenerated = GenerateMessage(seasonEnd, seasonStart, today, whichMessage, totalEpisodes);

        if (msgGenerated.IsNullOrWhiteSpace())
            msgGenerated = GenerateMessage(showInfo.SeasonEnd, showInfo.SeasonPremier, today, whichMessage, totalEpisodes);

        //// Priority 1: "Last Episode" if within 7 days of finale (future or today)
        //if (seasonEnd >= today && (seasonEnd - today).TotalDays <= 6)
        //    return FormatMessage("Last Episode", whichMessage, totalEpisodes);

        //// Priority 2: "Coming Soon" if within 7 days of premiere (future only)
        //if (seasonStart > today && (seasonStart - today).TotalDays <= 8)
        //    return FormatMessage("Coming Soon", whichMessage, totalEpisodes);

        //// In-season → episode number increments only on the release weekday
        //if (today >= seasonStart && today <= seasonEnd)
        //{
        //    int episodeNumber = CalculateEpisodeNumberAligned(seasonStart, today);

        //    if (episodeNumber < whichMessage)
        //        episodeNumber = whichMessage;

        //    // cap to actual season length, and ensure at least 1 while in-season
        //    if (episodeNumber < 1) episodeNumber = 1;
        //    if (episodeNumber > totalEpisodes) episodeNumber = totalEpisodes;

        //    return FormatMessage($"Episode {episodeNumber}", whichMessage, totalEpisodes);
        //}

        // Otherwise, no message
        return msgGenerated;
    }

    // — Helpers —

    // Nullable DateTime -> Date-only (fallback to today)
    private static DateTime GetDate(DateTime? input) => (input ?? DateTime.Today).Date;

    // Count episodes by advancing exactly on release weekday (every 7 days from premiere)
    private static int CalculateEpisodeNumberAligned(DateTime seasonStart, DateTime today)
    {
        int n = 1;                 // Episode 1 on the premiere day
        DateTime cursor = seasonStart;

        while (cursor.AddDays(7) <= today)
        {
            cursor = cursor.AddDays(7);
            n++;
        }
        return n;
    }

    private static string GenerateMessage(DateTime seasonEnd, DateTime seasonStart, DateTime today, int whichMessage, int totalEpisodes)
    {
        StringBuilder msgGenerated = new StringBuilder(256);
        // Priority 1: "Last Episode" if within 7 days of finale (future or today)
        if (seasonEnd >= today && (seasonEnd - today).TotalDays <= 6)
        {
            msgGenerated.Append(FormatMessage("Last Episode", whichMessage, totalEpisodes));
            return msgGenerated.ToString();
        }

        // Priority 2: "Coming Soon" if within 7 days of premiere (future only)
        if (seasonStart > today && (seasonStart - today).TotalDays <= 8)
        {
            msgGenerated.Append(FormatMessage("Coming Soon", whichMessage, totalEpisodes));
            return msgGenerated.ToString();
        }

        // In-season → episode number increments only on the release weekday
        if (today >= seasonStart && today <= seasonEnd)
        {
            int episodeNumber = CalculateEpisodeNumberAligned(seasonStart, today);

            if (episodeNumber < whichMessage)
                episodeNumber = whichMessage;

            // cap to actual season length, and ensure at least 1 while in-season
            if (episodeNumber < 1) episodeNumber = 1;
            if (episodeNumber > totalEpisodes) episodeNumber = totalEpisodes;

            msgGenerated.Append(FormatMessage($"Episode {episodeNumber}", whichMessage, totalEpisodes));
        }

        return msgGenerated.ToString();
    }

    // Optional formatter that can include total count depending on whichMessage flag
    // whichMessage: 0 = base text; 1 = include "of N" for episode messages; others = base
    private static string FormatMessage(string baseMessage, int whichMessage, int totalEpisodes)
    {
        if (whichMessage == 1 && baseMessage.StartsWith("Episode ", StringComparison.OrdinalIgnoreCase) && totalEpisodes > 0)
            return $"{baseMessage} of {totalEpisodes}";

        return baseMessage;
    }
}
