namespace Household.Shared.Models;

public sealed record DvdGenre
{
    public int Id { get; init; }

    public int MovieId { get; init; }

    public int TmdbId { get; init; }

    public required string Name { get; init; }

    public DvdMovieInformation? Movie { get; init; }
}
