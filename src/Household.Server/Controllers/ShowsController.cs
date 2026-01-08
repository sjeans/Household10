using Household.Application.Features.Shows.Commands;
using Household.Application.Features.Shows.Queries;
using Household.Application.Interfaces;
using Household.Shared.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Household.Server.Controllers;

[Produces("application/json")]
[ApiController]
[Route("/api/[controller]")]
public class ShowsController(IMediator mediator) : Controller, IShowsController
{
    [HttpPost]
    public async Task<string> CreateShow(TVShowDto request) => await mediator.Send(new CreateShow(request, mediator));

    [HttpGet]
    public async Task<List<TVShowDto>> GetAllShows() => await mediator.Send(new GetShows());

    [HttpGet("CompletedShows")]
    public async Task<List<TVShowDto>> GetAllCompletedShows() => await mediator.Send(new GetAllCompletedShows());

    [HttpGet("Episodes")]
    public async Task<List<int>?> GetEpisodeList() => await mediator.Send(new GetEpisodeList());

    [HttpGet("DayOfWeek/{dayOfWeek:int}")]
    public async Task<List<TVShowDto>> GetShowsByDayOfWeek(int dayOfWeek) => await mediator.Send(new GetShowsByDayOfWeek(dayOfWeek));

    [HttpGet("Details/{id:int}")]
    public async Task<TVShowDto> GetShowDetails(int id) => await mediator.Send(new GetShowDetails(id));

    [HttpPut("UpdateShow")]
    public async Task<string> PutTvShow(TVShowDto updatedShow) => await mediator.Send(new UpdateShow(updatedShow));

    [HttpDelete("RemoveShow/{id:int}")]
    public async Task<string> DeleteShow(int id) => await mediator.Send(new DeleteShow(id));
}
