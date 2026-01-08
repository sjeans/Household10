using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Episodes.Queries;

public class GetEpisodeDetailByAirDateRangeAsync(DateTime? startDate, DateTime? endDate) : IRequest<List<Episode>?>
{
    private DateTime? StartDate { get; } = startDate;
    private DateTime? EndDate { get; } = endDate;

    public class GetEpisodeDetailByAirDateRangeAsyncQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetEpisodeDetailByAirDateRangeAsync, List<Episode>?>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<List<Episode>?> Handle(GetEpisodeDetailByAirDateRangeAsync request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data!");

            List<Episode>? episodes = null;

            try
            {
                string startDate = string.Format("{0:yyyy-MM-dd}", request.StartDate);
                string endDate = string.Format("{0:yyyy-MM-dd}", request.EndDate);
                HttpResponseMessage response = await _client.GetAsync($"api/episodedetail/{startDate}/{endDate}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    Result<List<Episode>> result = await _jsonDeserializer.TryDeserializeAsync<List<Episode>>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    List<Episode>? foundEpisode = result.Value;

                    if (foundEpisode is not null)
                        episodes = foundEpisode;

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

            return episodes;
        }
    }
}
