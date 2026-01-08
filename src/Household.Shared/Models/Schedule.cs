using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Household.Shared.Models;

public class Schedule
{
    public int Id { get; set; }
    public int TvMazeId { get; set; }
    public int TvShowInformationId { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(50)]
    public string? Time { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(50)]
    public List<string>? Days { get; set; }

    public TvShowInformation? TvShowInformation { get; set; }
}
