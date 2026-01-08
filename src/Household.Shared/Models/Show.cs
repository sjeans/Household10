using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Household.Shared.Models;

public class Show
{
    public int Id { get; set; }
    public int TvMazeId { get; set; }
    public int? LinkId { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(250)]
    public string? Href { get; set; }

    public Link? Link { get; set; }
}
