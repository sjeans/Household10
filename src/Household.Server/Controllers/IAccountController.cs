using Microsoft.AspNetCore.Mvc;

namespace Household.Server.Interfaces;

public interface IAccountController
{
    Task<IActionResult> GetToken();
    IActionResult Login(string? returnUrl = "/");
    Task<IActionResult> Logout();
}