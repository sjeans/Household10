using System.ComponentModel.DataAnnotations;

namespace Household.Shared.Models;

public class Link
{
    public int Id { get; set; }

    [Required]
    public int TvMazeId { get; set; }
    public int? EpisodeId { get; set; }
    public int? TvShowinformationId { get; set; }
    public Self? Self { get; set; }
    public Previousepisode? PreviousEpisode { get; set; }
    public Show? Show { get; set; }

    public Episode? Episode { get; set; }
    public TvShowInformation? TvShowInformation { get; set; }
}
