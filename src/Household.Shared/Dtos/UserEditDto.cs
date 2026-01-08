using System.ComponentModel.DataAnnotations;
using Household.Shared.Models;

namespace Household.Shared.Dtos;

public class UserEditDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Must have a username.")]
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Must have a first name.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Must have a last name.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Must have an email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Must have a user type.")]
    public int UserTypeId { get; set; }
    public bool Active { get; set; }
    public UserType? UserType { get; set; }
}
