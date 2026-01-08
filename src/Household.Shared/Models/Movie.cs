namespace Household.Shared.Models;

public sealed class Movie
{
    public int MvId { get; set; }
    public string MvTitle { get; set; } = null!;
    public int MvDiskTypeId { get; set; }
    public bool MvCollectible { get; set; }
    public int MvDiskNum { get; set; }
    public bool Mv3D { get; set; }
    public bool Mv4K { get; set; }
}
