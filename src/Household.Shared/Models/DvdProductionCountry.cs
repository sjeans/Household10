namespace Household.Shared.Models;

public sealed record DvdProductionCountry
{
    public int Id { get; init; }

    public required string Iso3166_1 { get; init; }

    public required string Name { get; init; }

    public DvdMovieInformation? Movie { get; init; }
}
