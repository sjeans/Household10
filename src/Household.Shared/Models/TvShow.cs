namespace Household.Shared.Models;

public sealed class TvShow
{
    public TvShow()
    {
        StreamingService = new StreamingService();
    }

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DayOfWeek { get; set; }

    public DateTime Time { get; set; }

    public decimal Rating { get; set; }

    public int Episodes { get; set; }

    public bool IsCompletedSeason { get; set; }

    public bool IsCompleted { get; set; }

    public int Seasons { get; set; }

    public int StreamingServiceId { get; set; }

    public StreamingService StreamingService { get; set; }
}
