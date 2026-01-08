using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Household.Shared.Models;

public class Episode
{
    public int Id { get; set; }

    public int TvMazeId { get; set; }

    [Required]
    public int TvShowInformationId { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(250)]
    public string? Url { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;
    public int? Season { get; set; }
    public int? Number { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(50)]
    public string? Type { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(50)]
    public string? AirDate { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(50)]
    public string? AirTime { get; set; }
    public DateTime? AirStamp { get; set; }
    public int? Runtime { get; set; }
    public Rating? Rating { get; set; }
    public Image? Images { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(2000)]
    public string? Summary { get; set; }
    public Link? Links { get; set; }

    public TvShowInformation TvShowInformation { get; set; } = default!;
}
