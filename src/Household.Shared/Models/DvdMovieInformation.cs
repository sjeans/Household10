namespace Household.Shared.Models;

public sealed record DvdMovieInformation
{
    public int Id { get; init; }

    public int MovieId { get; init; }

    public int TmdbId { get; init; }

    public bool Adult { get; init; }

    public string? BackdropPath { get; init; }

    public List<DvdGenre> Genres { get; init; } = [];

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

    public DateTime? ReleaseDate { get; init; }

    public double Revenue { get; init; }

    public string? Runtime { get; init; }

    public string Released { get; init; } = string.Empty;

    public List<DvdSpokenLanguage> SpokenLanguages { get; init; } = [];

    public string TagLine { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public bool Video { get; init; }

    public double VoteAverage { get; init; }

    public int VoteCount { get; init; }
}
