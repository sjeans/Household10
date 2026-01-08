namespace Household.Shared.Models;

public sealed class DigitalDownload
{
    public int Id { get; set; }
    public int MvId { get; set; }
    public bool? HasDownload { get; set; }
    public bool? Downloaded { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? DownloadDate { get; set; }
}
