using Household.Shared.Dtos;

namespace Household.Application.Interfaces;

public interface IShowsController
{
    Task<string> CreateShow(TVShowDto request);
    Task<string> DeleteShow(int id);
    Task<List<TVShowDto>> GetAllCompletedShows();
    Task<List<TVShowDto>> GetAllShows();
    Task<List<int>?> GetEpisodeList();
    Task<TVShowDto> GetShowDetails(int id);
    Task<List<TVShowDto>> GetShowsByDayOfWeek(int dayOfWeek);
    Task<string> PutTvShow(TVShowDto updatedShow);
}
