using Household.Application.Features.Users.Commands;
using Household.Application.Features.Users.Queries;
using Household.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Household.Server.Controllers;

//[EnableCors("CorsPolicy")]
[Produces("application/json")]
[ApiController]
[Route("/api/[controller]")]
public class UserController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<List<User>> GetAllUsersAsync() => await mediator.Send(new GetAllUsers());

    [HttpGet("{id}")]
    public async Task<User?> GetUserById(int id) => await mediator.Send(new GetUserById(id));

    [HttpPut("UpdateUser")]
    public async Task<string> PutUser(User updatedUser) => await mediator.Send(new UpdateUser(updatedUser));

    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
