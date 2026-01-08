using Household.Shared.Enums;
using Household.Shared.Models;

using System.ComponentModel.DataAnnotations;

namespace Household.Shared.Dtos;

public class MovieInfoDto
{
    [Required(ErrorMessage = "Title is required field")]
    public string Title { get; set; } = null!;
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Description { get; set; }
    public bool HasDownload { get; set; }
    public bool Downloaded { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? DownloadDate { get; set; }
    public bool Collectible { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Select number of disks")]
    public int DiskNum { get; set; }
    public bool Is3D { get; set; }
    public bool Is4K { get; set; }
    public string? CheckedoutTo { get; set; }
    public int Id { get; set; }

    //[EnumDataType(typeof(DvdTypeDto))]
    [Range(1, int.MaxValue, ErrorMessage = "Select disk type")]
    public DvdTypes DvdType { get; set; }
    public User? UserInfo { get; set; }
    public DigitalDownload? DigitalDownload { get; set; }
    public CheckedOut? Checkout { get; set; }
}
