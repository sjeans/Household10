using Household.Shared.Dtos;
using Household.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Household.Application.Interfaces;

public interface ITvShowInformationController
{
    Task<List<TVShowInformationDto>?> GetTvShowInformationByName(string name);
    Task<Dictionary<int, string>?> GetTvShowInformationNames();
    Task<List<TVShowInformationDto>?> GetTvShowInformationByNameList(List<string> names);
    Task<string> CreateTvShowInformation(TvShowInformation request);
    Task<string> PutUpdateShowEpisode(int id, [FromBody] Dictionary<string, int> valuePairs);
    Task<string> PutAddShowEpisode(int id, [FromBody] Dictionary<string, int> valuePairs);
}
