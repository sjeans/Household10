using System.ComponentModel.DataAnnotations;

namespace Household.Shared.Models;

public sealed class MovieInfo
{
    [Required]
    public string MvTitle { get; set; } = null!;

    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Description { get; set; }
    public bool? HasDownload { get; set; }
    public bool? Downloaded { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? DownloadDate { get; set; }

    [Required]
    public bool MvCollectible { get; set; }

    [Required]
    public int MvDiskNum { get; set; }

    [Required]
    public bool Mv3D { get; set; }

    [Required]
    public bool Mv4K { get; set; }

    [Required]
    public string? CheckedoutTo { get; set; }

    [Required]
    public int MvId { get; set; }

    public int? Dtid { get; set; }
    public int? Uid { get; set; }
    public int? Ddid { get; set; }
    public int? Coid { get; set; }
}
