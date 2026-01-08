using Household.Application.Features.MovieInformation.Queries;
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
public class MovieInfoController(IMediator mediator) : Controller, IMovieInfoController
{
    [HttpGet]
    public async Task<List<MovieInfoDto>> MoviesAsync() => await mediator.Send(new GetAllMovieInformation());

    [HttpGet("{id:int}")]
    public async Task<MovieInfoDto> MoviesAsync(int id) => await mediator.Send(new GetMovieInformationById(id));
}
