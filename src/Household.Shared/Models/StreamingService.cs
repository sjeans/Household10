using System.ComponentModel.DataAnnotations;

namespace Household.Shared.Models;

public sealed class StreamingService
{
    public StreamingService()
    {
        TvShows = new List<TvShow>();
    }

    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required field")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Subscription { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? PaySchedule { get; set; }

    public ICollection<TvShow> TvShows { get; set; }
}
