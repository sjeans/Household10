using System.ComponentModel.DataAnnotations;
using Household.Shared.Helpers;

namespace Household.Shared.Dtos;

public class StreamingServiceDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required field")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Subscription { get; set; }

    public decimal? Amount { get; set; }

    public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-365);

    [Required(ErrorMessage = "Need to select a date")]
    [DateGreaterThan("StartDate")]
    public DateTime? PaySchedule { get; set; }
}
