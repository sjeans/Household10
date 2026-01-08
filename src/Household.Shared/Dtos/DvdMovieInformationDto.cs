using Household.Shared.Enums;
using Household.Shared.Models;

namespace Household.Shared.Dtos;

public sealed class DvdMovieInformationDto
{
    public int Id { get; init; }

    public int MovieId { get; init; }

    public int TmdbId { get; init; }

    public bool Adult { get; init; }

    public string? BackdropPath { get; init; }

    public string? Genres { get; init; } = string.Empty;

    public string? HomePage { get; init; }

    public string? ImdbId { get; init; }

    public DvdMovieCollection? MovieCollections { get; init; }

    public List<string> OriginCountries { get; init; } = [];

    public string OriginalLanguage { get; init; } = string.Empty;

    public string OriginalTitle { get; init; } = string.Empty;

    public string Overview { get; init; } = string.Empty;

    public double Popularity { get; init; }

    public string? PosterPath { get; init; }

    public List<DvdProductionCompany> ProductionCompanies { get; init; } = [];

    public List<DvdProductionCountry> ProductionCountries { get; init; } = [];

    public string? ReleaseDate { get; init; }

    public string? Revenue { get; init; }

    public string? Runtime { get; init; }

    public string Released { get; init; } = string.Empty;

    public List<DvdSpokenLanguage> SpokenLanguages { get; init; } = [];

    public string TagLine { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public bool Video { get; init; }

    public double VoteAverage { get; init; }

    public int VoteCount { get; init; }

    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Description { get; set; }
    public bool HasDownload { get; set; }
    public bool Downloaded { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? DownloadDate { get; set; }
    public bool Collectible { get; set; }

    public int DiskNum { get; set; }
    public bool Is3D { get; set; }
    public bool Is4K { get; set; }
    public string? CheckedoutTo { get; set; }

    public DvdTypes DvdType { get; set; }
    public User? UserInfo { get; set; }
    public DigitalDownload? DigitalDownload { get; set; }
    public CheckedOut? Checkout { get; set; }
}
