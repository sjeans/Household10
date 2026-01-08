namespace Household.Shared.Models;

public sealed record DvdSpokenLanguage
{
    public int Id { get; init; }

    public required string EnglishName { get; init; }

    public required string Iso639_1 { get; init; }

    public required string Name { get; init; }

    public DvdMovieInformation? Movie { get; init; }
}
