using System.ComponentModel.DataAnnotations;
using Household.Shared.Enums;

namespace Household.Shared.Dtos;

public class TVShowDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required field")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Day of week is required field")]
    public DayOfWeek DayOfWeek { get; set; }

    public string Time { get; set; } = string.Empty;

    public string StartDate { get; set; } = string.Empty;

    public decimal Rating { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Select number of episodes")]
    public int Episodes { get; set; }

    public bool IsCompletedSeason { get; set; }

    public bool IsCompleted { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Select number of seasons")]
    public Seasons Season { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Select a streaming service")]
    public int StreamingId { get; set; }

    public string StreamingName { get; set; } = string.Empty;

    public string StreamingDescription { get; set; } = string.Empty;

    public string StreamingSubscription { get; set; } = string.Empty;
    //public StreamingService StreamingService { get; set; } = new ();
}
