using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Household.Shared.Models;

public class Image
{
    public int Id { get; set; }

    public int TvMazeId { get; set; }

    public int? EpisodeId { get; set; }
    public int? TvShowInformationId { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(250)]
    public string? Medium { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(250)]
    public string? Original { get; set; }

    public Episode? Episode { get; set; }
    public TvShowInformation? TvShowInformation { get; set; }
}
