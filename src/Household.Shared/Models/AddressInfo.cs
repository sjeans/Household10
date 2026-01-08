namespace Household.Shared.Models;

public sealed class AddressInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }

    public ICollection<ContactNumber>? ContactNumbers { get; set; }
}
