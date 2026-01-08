namespace Household.Shared.Models;

public sealed class UserType
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public User? User { get; set; }
}
