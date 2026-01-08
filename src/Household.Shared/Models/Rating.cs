namespace Household.Shared.Models;

public class Rating
{
    public int Id { get; set; }
    public int TvMazeId { get; set; }
    public int? EpisodeId { get; set; }
    public int? TvShowInformationId { get; set; }
    public double? Average { get; set; }

    public Episode? Episode { get; set; }
    public TvShowInformation? TvShowInformation { get; set; }
}
