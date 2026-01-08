using System.ComponentModel.DataAnnotations;

namespace Household.Shared.Dtos;

public class AddressInfoEditDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Address must belong to someone.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Address cannot be empty.")]
    public string Address { get; set; } = null!;
    public string? Address2 { get; set; }

    [Required(ErrorMessage = "City cannot be empty.")]
    public string? City { get; set; }

    [Required(ErrorMessage = "State cannot be empty.")]
    public string? State { get; set; }

    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }

    public ICollection<ContactNumberDto>? ContactNumbers { get; set; }
}
