using Household.Application.Features.ContactNumbers.Commands;
using Household.Application.Features.ContactNumbers.Queries;
using Household.Application.Interfaces;
using Household.Shared.Dtos;
using MediatR;
//using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Household.Server.Controllers;

//[EnableCors("CorsPolicy")]
[Produces("application/json")]
[ApiController]
[Route("/api/[controller]")]
public class ContactNumberController(IMediator mediator) : ControllerBase, IContactNumberController
{
    [HttpPost]
    public async Task<string> CreateContactNumber(ContactNumberDto request) => await mediator.Send(new CreateContactNumber(request));

    [HttpGet]
    public async Task<List<ContactNumberDto>> GetAllContactNumbers() => await mediator.Send(new GetAllContactNumbers());

    [HttpGet("{id}")]
    public async Task<ContactNumberDto> GetContactById(int id) => await mediator.Send(new GetContactById(id));

    [HttpPut("Update")]
    public async Task<string> PutContactNumber(ContactNumberDto updatedContact) => await mediator.Send(new UpdateContactNumber(updatedContact));
}
