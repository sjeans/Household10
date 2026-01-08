using Household.Shared.Dtos;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Household.Server.Controllers;

[Produces("application/json")]
[ApiController]
[Route("/api/[controller]")]
public class UserIpServiceController : ControllerBase
{
    private readonly IUserIpService _userIpService;

    public UserIpServiceController(IUserIpService userIpService)
    {
        _userIpService = userIpService;
    }

    [HttpGet("GetIpAddress")]
    public IActionResult GetIpAddress()
    {
        _userIpService.GetUserIP();
        return Ok(
            new UserIpDto()
            {
                IpAddress = _userIpService.IpAddress,
                LogMessage = _userIpService.LogMessage,
                CanShow = _userIpService.CanShow,
                CanSave = _userIpService.CanSave,
                DisableButton = _userIpService.DisableButton,
                PermissionSetBy = _userIpService.PermissionSetBy,
            });
    }
}
