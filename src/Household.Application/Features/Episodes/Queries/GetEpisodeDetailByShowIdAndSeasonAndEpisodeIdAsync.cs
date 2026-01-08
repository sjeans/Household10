using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Episodes.Queries;

public class GetEpisodeDetailByShowIdAndSeasonAndEpisodeIdAsync(int showId, int season, int episodeId) : IRequest<Episode?>
{
    public int ShowId { get; } = showId;
    public int Season { get; } = season;
    public int EpisodeId { get; } = episodeId;

    public class GetEpisodeDetailByShowIdAndSeasonAndEpisodeIdAsyncQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetEpisodeDetailByShowIdAndSeasonAndEpisodeIdAsync, Episode?>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<Episode?> Handle(GetEpisodeDetailByShowIdAndSeasonAndEpisodeIdAsync request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data!");

            Episode? episode = null;

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/episodedetail/{request.ShowId}/{request.Season}/{request.EpisodeId}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    Result<Episode> result = await _jsonDeserializer.TryDeserializeAsync<Episode>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    Episode? foundEpisode = result.Value;

                    if (foundEpisode is not null)
                        episode = foundEpisode;

                }
            }
            catch (HttpRequestException hrex)
            {
                throw new BadRequestException(hrex.GetInnerMessage(), hrex);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.GetInnerMessage(), ex);
            }

            return episode;
        }
    }
}
