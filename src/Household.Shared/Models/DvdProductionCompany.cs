namespace Household.Shared.Models;

public sealed record DvdProductionCompany
{
    public int Id { get; init; }

    public int TmdbId { get; init; }

    public string? LogoPath { get; init; }

    public required string Name { get; init; }

    public required string OriginCountry { get; init; }

    public DvdMovieInformation? Movie { get; init; }
}
