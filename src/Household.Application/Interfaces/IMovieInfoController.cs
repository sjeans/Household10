using Household.Shared.Dtos;

namespace Household.Application.Interfaces;

public interface IMovieInfoController
{
    Task<List<MovieInfoDto>> MoviesAsync();
    Task<MovieInfoDto> MoviesAsync(int id);
}