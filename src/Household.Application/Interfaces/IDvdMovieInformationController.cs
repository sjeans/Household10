using Household.Shared.Models;

namespace Household.Application.Interfaces;

public interface IDvdMovieInformationController
{
    Task<List<DvdMovieInformation>?> GetAllDvdMovieInformation();
    Task<DvdMovieInformation?> GetAllDvdMovieInformationById(int id);
    Task<List<DvdMovieInformation>?> GetAllDvdMovieInformationByIds(List<int> ids);
    Task<DvdMovieInformation?> GetAllDvdMovieInformationByImdbId(string imdbId);
    Task<DvdMovieInformation?> GetAllDvdMovieInformationByName(string name);
    Task<List<DvdMovieInformation>?> GetAllDvdMovieInformationByTmdbId(int tmdbId);
    Task<List<DvdMovieInformation>?> GetAllDvdMovieInformationNames(List<string> names);
}
