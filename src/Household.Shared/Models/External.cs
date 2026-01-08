using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Household.Shared.Models;

public class External
{
    public int Id { get; set; }
    public int TvMazeId { get; set; }
    public int? TvShowInformationId { get; set; }
    public long? TvRage { get; set; }
    public int? TheTvDb { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(50)]
    public string? Imdb { get; set; }

    public TvShowInformation TvShowInformation { get; set; } = default!;
}
