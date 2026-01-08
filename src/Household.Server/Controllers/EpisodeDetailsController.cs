using Household.Application.Features.Episodes.Queries;
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
public class EpisodeDetailsController(IMediator mediator) : Controller
{
    //[HttpPost]
    //public async Task<string> CreateEpisode(TVShowDto request) => await mediator.Send(new CreateEpisodeAsync(request));

    [HttpGet]
    public async Task<List<Episode>?> GetAllEpisodes() => await mediator.Send(new GetAllEpisodesAsync());

    [HttpGet("LiveEpisodes/{showTvMazeId:int}")]
    public async Task<List<Episode>?> GetAllLiveEpisodes(int showTvMazeId) => await mediator.Send(new GetAllLiveEpisodesAsync(showTvMazeId));

    //[HttpGet("CompletedShows")]
    //public async Task<List<TVShowDto>> GetAllCompletedShows() => await mediator.Send(new GetAllCompletedShows());

    //[HttpGet("{showId:int}/Episodes")]
    //public async Task<List<int>?> GetEpisodeList(int showId) => await mediator.Send(new GetEpisodeListAsync(showId));

    [HttpGet("{showId:int}/{season:int}/{episodeId:int}")]
    public async Task<Episode?> GetEpisodeDetailByShowIdAndSeasonAndEpisodeId(int showId, int season, int episodeId) => await mediator.Send(new GetEpisodeDetailByShowIdAndSeasonAndEpisodeIdAsync(showId, season, episodeId));

    [HttpGet("ThisWeeksEpisodes/{startDate:datetime}/{endDate:datetime}")]
    public async Task<List<Episode>?> GetEpisodeDetailByAirDateRange(DateTime startDate, DateTime endDate) => await mediator.Send(new GetEpisodeDetailByAirDateRangeAsync(startDate, endDate));

    [HttpGet("ShowSeasons/{showId:int}")]
    public async Task<Dictionary<int, string>?> GetShowSeasonInformation(int showId)
    {
        var retVar = await mediator.Send(new GetShowSeasonInformationAsync(showId));
    // https://api.tvmaze.com/shows/45840/seasons
        return retVar;
    }

    //[HttpGet("Details/{id:int}")]
    //public async Task<TVShowDto> GetShowDetails(int id) => await mediator.Send(new GetShowDetails(id));

    //[HttpPut("UpdateShow")]
    //public async Task<string> PutTvShow(TVShowDto updatedShow) => await mediator.Send(new UpdateShow(updatedShow));

    //[HttpDelete("RemoveShow/{id:int}")]
    //public async Task<string> DeleteShow(int id) => await mediator.Send(new DeleteShow(id));
}
