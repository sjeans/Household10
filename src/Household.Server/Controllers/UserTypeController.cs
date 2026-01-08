using Household.Application.Features.UserTypes.Queries;
using Household.Application.Interfaces;
using Household.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Household.Server.Controllers;

//[EnableCors("CorsPolicy")]
[Produces("application/json")]
[ApiController]
[Route("/api/[controller]")]
public class UserTypeController(IMediator mediator) : Controller, IUserTypeController
{
    [HttpGet]
    public async Task<List<UserType>> GetAllUserTypesAsync() => await mediator.Send(new GetAllUserTypes());

    [HttpGet("{id}")]
    public async Task<UserType?> GetUserTypeById(int id) => await mediator.Send(new GetUserTypeById(id));
}
