using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Household.Shared.Models;

public class TvShowInformation
{
    public int Id { get; set; }
    public int TvMazeId { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(250)]
    public string? Url { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "varchar")]
    [StringLength(50)]
    public string? Type { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(10)]
    public string? Language { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(50)]
    public List<string>? Genres { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(50)]
    public string? Status { get; set; }
    public int? Runtime { get; set; }
    public int? AverageRuntime { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(50)]
    public string? Premiered { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(50)]
    public string? Ended { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(250)]
    public string? OfficialSite { get; set; }
    public Schedule? Schedule { get; set; }
    public Rating? Rating { get; set; }
    public int? Weight { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(2000)]
    public string? Network { get; set; }
    public WebChannel? WebChannel { get; set; }
    public string? DvdCountry { get; set; }
    public External? Externals { get; set; }
    public Image? Images { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(2000)]
    public string? Summary { get; set; }
    public int? Updated { get; set; }
    public Link? Links { get; set; }

    public List<Episode>? Episodes { get; set; }
}
