using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Episodes.Queries;

public class GetAllLiveEpisodesAsync(int showTvMazeId) : IRequest<List<Episode>?>
{
    private int ShowTvMazeId { get; } = showTvMazeId;

    public class GetAllLiveEpisodesAsyncQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer jsonDeserializer) : IRequestHandler<GetAllLiveEpisodesAsync, List<Episode>?>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private IAppJsonDeserializer _jsonDeserializer = jsonDeserializer;

        public async Task<List<Episode>?> Handle(GetAllLiveEpisodesAsync request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot retrieve data!");

            List<Episode>? shows = [];

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/episodedetail/liveepisodes/{request.ShowTvMazeId}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    Result<List<Episode>> result = await _jsonDeserializer.TryDeserializeAsync<List<Episode>>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    List<Episode>? tvShows = result.Value;

                    if (tvShows!.Count != 0)
                        shows = tvShows;

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

            return shows;
        }
    }
}
