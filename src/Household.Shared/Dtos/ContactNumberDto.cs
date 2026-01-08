using System.ComponentModel.DataAnnotations;

namespace Household.Shared.Dtos;

public class ContactNumberDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter a useful type of phone number. (Home, Work, or Cell)")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Please enter a phone number.")]
    public string PhoneNumber { get; set; } = null!;
}
