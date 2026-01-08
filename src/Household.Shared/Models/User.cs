namespace Household.Shared.Models;

public sealed class User
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public int UserTypeId { get; set; }
    public bool Active { get; set; }
    public UserType? UserType { get; set; }
}
