using Household.Application.Features.Addresses.Commands;
using Household.Application.Features.Addresses.Queries;
using Household.Application.Interfaces;
using Household.Shared.Dtos;
using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Household.Server.Controllers;

//[EnableCors("CorsPolicy")]
[Produces("application/json")]
[ApiController]
[Route("/api/[controller]")]
public class AddressBookController(IMediator mediator) : ControllerBase, IAddressBookController
{
    [HttpPost]
    public async Task<string> CreateShow(AddressInfoDto request) => await mediator.Send(new CreateAddressBookCommand(request));

    [HttpGet]
    public async Task<List<AddressInfoDto>> GetAllAddresses() => await mediator.Send(new GetAllAddresses());

    [HttpGet("{id:int}")]
    public async Task<AddressInfoDto> GetAddressById(int id) => await mediator.Send(new GetAddressById(id));

    [HttpGet("{letter}")]
    public async Task<List<AddressInfoDto>> GetAddressByFirstLetter(string letter) => await mediator.Send(new GetAddressByFirstLetter(letter));

    [HttpPut("Update")]
    public async Task<string> PutAddressBook(AddressInfoDto updatedAddress) => await mediator.Send(new UpdateAddressBookCommand(updatedAddress));
}
