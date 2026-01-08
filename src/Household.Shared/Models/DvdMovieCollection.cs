namespace Household.Shared.Models;

public sealed record DvdMovieCollection
{
    public int Id { get; init; }

    public int TmdbId { get; init; }

    public required string Name { get; init; }

    public string? PosterPath { get; init; }

    public string? BackdropPath { get; init; }

    public List<DvdMovieInformation> Movies { get; init; } = [];
}
