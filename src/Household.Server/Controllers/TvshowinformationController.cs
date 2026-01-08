using Household.Shared.Dtos;
using Household.Shared.Models;
using Household.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Household.Application.Features.TvShowInformations.Queries;
using Household.Application.Features.TvShowInformations.Commands;
using Household.Application.Features.Episodes.Commands;
using MediatR;

namespace Household.Server.Controllers;

[Produces("application/json")]
[ApiController]
[Route("/api/[controller]")]
public class TvShowInformationController(IMediator mediator) : Controller, ITvShowInformationController
{
    [HttpPost]
    public async Task<string> CreateTvShowInformation(TvShowInformation request) => await mediator.Send(new CreateTvShowInformationAsync(request));

    //[HttpGet]
    //public async Task<List<StreamingServiceDto>> GetAllSubscriptions() => await mediator.Send(new GetAllSubscriptions());

    [HttpGet("Names")]
    public async Task<Dictionary<int, string>?> GetTvShowInformationNames() => await mediator.Send(new GetTvShowInformationNames());

    [HttpGet("{name}")]
    public async Task<List<TVShowInformationDto>?> GetTvShowInformationByName(string name) => await mediator.Send(new GetTvShowInformationByName(name));

    [HttpPost("ShowInformation")]
    public async Task<List<TVShowInformationDto>?> GetTvShowInformationByNameList(List<string> names) => await mediator.Send(new GetTvShowInformationByNameList(names));

    [HttpGet("RetrieveShowInformation/{showName}/{createNew:bool}")]
    public async Task<TvShowInformation?> RetrieveTvShowInformationByName(string showName, bool createNew = false) => await mediator.Send(new RetrieveTvShowInformationByName(showName, createNew));

    //[HttpGet("Shows/{id}")]
    //public async Task<List<TVShowDto>> GetAllShowsByServiceId(int id) => await mediator.Send(new GetAllShowsByServiceId(id));

    [HttpPut("UpdateShowEpisode/{id}")]
    public async Task<string> PutUpdateShowEpisode(int id, [FromBody] Dictionary<string, int> valuePairs) => await mediator.Send(new UpdateTvShowInformationAsync(id, valuePairs));

    [HttpPut("AddShowEpisode/{id}")]
    public async Task<string> PutAddShowEpisode(int id, [FromBody] Dictionary<string, int> valuePairs) => await mediator.Send(new AddEpisodeInformationAsync(id, valuePairs));

    //[HttpDelete("RemoveSubscription/{id:int}")]
    //public async Task<string> DeleteSubscription(int id) => await mediator.Send(new DeleteSubscription(id));
}
