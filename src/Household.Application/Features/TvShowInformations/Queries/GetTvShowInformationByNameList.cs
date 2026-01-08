using System.Net.Http.Json;
using Household.Application.Features.Episodes.Queries;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Enums;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;
using Serilog;

namespace Household.Application.Features.TvShowInformations.Queries;

public class GetTvShowInformationByNameList(List<string> names) : IRequest<List<TVShowInformationDto>?>
{
    public List<string> Names { get; } = names;

    public class GetTvShowInformationByNameListQueryHandler(ILogger logger, IHttpClientFactory httpClientFactory, IAppJsonDeserializer jsonDeserializer) : IRequestHandler<GetTvShowInformationByNameList, List<TVShowInformationDto>?>
    {
        private readonly ILogger _logger = logger.ForContext<GetTvShowInformationByNameList>();
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IAppJsonDeserializer _jsonDeserializer = jsonDeserializer;

        public async Task<List<TVShowInformationDto>?> Handle(GetTvShowInformationByNameList request, CancellationToken cancellationToken)
        {
            if (_client == null)
            {
                _logger.Error("HttpClient is null in GetTvShowInformationByNameListQueryHandler.");
                throw new BadRequestException("Cannot retrieve data!");
            }

            List<TVShowInformationDto> tvShowInformations = new();

            try
            {
                HttpResponseMessage response = await _client.PostAsJsonAsync("api/ShowInformation/GetAllShowInformation", request.Names, cancellationToken);

                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<List<TvShowInformation>> result = await _jsonDeserializer.TryDeserializeAsync<List<TvShowInformation>>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    // Handle error gracefully
                    _logger.Error("Deserialization failed in GetTvShowInformationByNameListQueryHandler: {Error}", result.Error);
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                List<TVShowInformationDto> tvShows = 
                    result.Value != null ? [.. result.Value.Select(MapDbToDto)] : [];

                if (tvShows!.Count != 0)
                    tvShowInformations = tvShows;

                foreach (TVShowInformationDto tVShow in tvShows)
                {
                    //
                    GetShowSeasonInformationAsync tmp = new(tVShow.TvMazeId);
                    GetShowSeasonInformationAsync.GetShowSeasonInformationAsyncQueryHandler temp = new(_logger, _client, _jsonDeserializer);
                    Dictionary<int, string>? seasonInformation = await temp.Handle(tmp, cancellationToken);

                    if (seasonInformation is not null)
                    {
                        if (DateTime.TryParse(seasonInformation.GetValueOrDefault(1), out DateTime seasonPremier))
                            tVShow.SeasonPremier = seasonPremier;

                        if (DateTime.TryParse(seasonInformation.GetValueOrDefault(2), out DateTime seasonEnd))
                            tVShow.SeasonEnd = seasonEnd;
                    }
                }
            }
            catch (HttpRequestException hrex)
            {
                _logger.Error("HTTP request error in GetTvShowInformationByNameListQueryHandler: {Message}", hrex.Message);
                throw new BadRequestException(hrex.GetInnerMessage(), hrex);
            }
            catch (Exception ex)
            {
                _logger.Error("Unexpected error in GetTvShowInformationByNameListQueryHandler: {Message}", ex.Message);
                throw new BadRequestException(ex.GetInnerMessage(), ex);
            }

            return tvShowInformations;
        }

        private TVShowInformationDto MapDbToDto(TvShowInformation dto)
        {
            List<Episode>? episodes = dto.Episodes!;

            string? startDate = null;
            int season = 1;
            if (dto.Episodes?.Count > 0)
            {
                startDate = episodes[0].AirStamp.HasValue ? episodes[0]?.AirStamp!.Value.ToShortDateString() : string.Empty;
                season = episodes[^1].Season.HasValue ? episodes[^1]!.Season!.Value : 0;
            }

            TVShowInformationDto tvShow = new()
            {
                Id = dto.Id,
                Image = new()
                {
                    Medium = dto.Images?.Medium ?? string.Empty,
                    Original = dto.Images?.Original ?? string.Empty,
                },
                NumberEpisodes = episodes.Count,
                Name = dto.Name!,
                Rating = dto.Rating?.Average ?? 0,
                StartDate = startDate ?? string.Empty,
                Season = (Seasons)season,
                Summary = dto.Summary,
                OfficialSite = dto.OfficialSite,
                AverageRuntime = dto.AverageRuntime,
                ShowType = dto.Type,
                Genres = dto.Genres,
                Premiered = dto.Premiered,
                Ended = dto.Ended,
                TvMazeId = dto.TvMazeId,
            };

            if (dto.Schedule?.Days?.Count > 0)
            {
                tvShow.DayOfWeek = dto?.Schedule?.Days?.FirstOrDefault() switch
                {
                    "Sunday" => DayOfWeek.Sunday,
                    "Monday" => DayOfWeek.Monday,
                    "Tuesday" => DayOfWeek.Tuesday,
                    "Wednesday" => DayOfWeek.Wednesday,
                    "Thursday" => DayOfWeek.Thursday,
                    "Friday" => DayOfWeek.Friday,
                    "Saturday" => DayOfWeek.Saturday,
                    _ => throw new NotImplementedException("Scheduled days is not set!")
                };
            }

            tvShow.Episodes = [.. episodes];

            return tvShow;
        }
    }
}
