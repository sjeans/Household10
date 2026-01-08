using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Household.Shared.Models;

public class WebChannel
{
    public int Id { get; set; }

    [Required]
    public int TvMazeId { get; set; }
    public int TvShowInformationId { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(50)]
    public string? Name { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(2000)]
    public string? Country { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(250)]
    public string? OfficialSite { get; set; }

    public TvShowInformation? TvShowInformation { get; set; }
}
