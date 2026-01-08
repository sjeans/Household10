using Household.Application.Interfaces;
using Household.Application.Features.DvdMovieInformation.Queries;
using Household.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Household.Server.Controllers;

//[EnableCors("CorsPolicy")]
[Produces("application/json")]
[ApiController]
[Route("/api/[controller]")]
public class DvdMovieInformationController(IMediator mediator) : ControllerBase, IDvdMovieInformationController
{
    public IMediator _mediator { get; } = mediator;

    [HttpGet]
    public async Task<List<DvdMovieInformation>?> GetAllDvdMovieInformation() => await _mediator.Send(new GetAllDvdMovieInformation());

    [HttpPost("GetAllMovieInformationTitles")]
    public async Task<List<DvdMovieInformation>?> GetAllDvdMovieInformationNames(List<string> names) => await _mediator.Send(new GetAllDvdMovieInformationByNames(names));

    [HttpPost("GetAllMovieInformationIds")]
    public async Task<List<DvdMovieInformation>?> GetAllDvdMovieInformationByIds(List<int> ids) => await _mediator.Send(new GetAllDvdMovieInformationByIds(ids));

    [HttpGet("{id:int}")]
    public async Task<DvdMovieInformation?> GetAllDvdMovieInformationById(int id) => await _mediator.Send(new GetDvdMovieInformationById(id));

    [HttpGet("tmdb/{tmdbId:int}")]
    public async Task<List<DvdMovieInformation>?> GetAllDvdMovieInformationByTmdbId(int tmdbId) => await _mediator.Send(new GetDvdMovieInformationByTmdbId(tmdbId));

    [HttpGet("imdb/{imdbId}")]
    public async Task<DvdMovieInformation?> GetAllDvdMovieInformationByImdbId(string imdbId) => await _mediator.Send(new GetDvdMovieInformationByImdbId(imdbId));

    [HttpGet("title/{name}")]
    public async Task<DvdMovieInformation?> GetAllDvdMovieInformationByName(string name) => await _mediator.Send(new GetDvdMovieInformationByName(name));
}
