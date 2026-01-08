using Household.Shared.Enums;

namespace Household.Shared.Dtos;

public class TVShowInformationDto
{
    public int Id { get; set; }

    public int TvMazeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public string Time { get; set; } = string.Empty;

    public string StartDate { get; set; } = string.Empty;

    public double Rating { get; set; }

    public int NumberEpisodes { get; set; }

    public List<Models.Episode>? Episodes { get; set; }

    public Models.Image? Image { get; set; }

    public string? OfficialSite { get; set; }

    public int? AverageRuntime { get; set; }

    public List<string>? Genres { get; set; }

    public string? ShowType { get; set; }

    public string? Premiered { get; set; }
    
    public string? Ended { get; set; }

    public bool IsCompletedSeason { get; set; }

    public bool IsCompleted { get; set; }

    public Seasons Season { get; set; }

    public DateTime SeasonPremier { get; set; }

    public DateTime SeasonEnd { get; set; }

    public int StreamingId { get; set; }

    public string StreamingName { get; set; } = string.Empty;

    public string StreamingDescription { get; set; } = string.Empty;

    public string StreamingSubscription { get; set; } = string.Empty;
}
