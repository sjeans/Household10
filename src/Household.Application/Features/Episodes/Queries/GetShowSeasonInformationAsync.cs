using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using MediatR;
using Serilog;

namespace Household.Application.Features.Episodes.Queries;

public class GetShowSeasonInformationAsync(int showId) : IRequest<Dictionary<int, string>?>
{
    private readonly int ShowId = showId;

    public class GetShowSeasonInformationAsyncQueryHandler(ILogger logger, HttpClient httpClient, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetShowSeasonInformationAsync, Dictionary<int, string>?>
    {
        private readonly ILogger _logger = logger.ForContext<GetShowSeasonInformationAsync>();
        private readonly HttpClient _client = httpClient;
        private readonly IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<Dictionary<int, string>?> Handle(GetShowSeasonInformationAsync request, CancellationToken cancellationToken)
        {
            if (_client == null)
            {
                _logger.Error("HttpClient is null in GetShowSeasonInformationAsyncQueryHandler");
                throw new BadRequestException("Cannot make client call to retrieve data!");
            }

            try
            {//https://api.tvmaze.com/shows/45840/seasons
                HttpResponseMessage response = await _client.GetAsync($"https://api.tvmaze.com/shows/{request.ShowId}/seasons", cancellationToken);

                string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<List<ShowSeason>> result = await _jsonDeserializer.TryDeserializeAsync<List<ShowSeason>>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    _logger.Error("Deserialization failed in GetShowSeasonInformationAsyncQueryHandler: {Error}", result.Error);
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                List<ShowSeason>? tvShows = result.Value;
                Dictionary<int, string> valuePairs = [];

                if (tvShows!.Count != 0)
                    foreach (ShowSeason season in tvShows.OrderByDescending(o => o.number))
                    {
                        if (!season.premiereDate.IsNullOrWhiteSpace() && !season.endDate.IsNullOrWhiteSpace())
                        {
                            valuePairs.Add(1, season.premiereDate);
                            valuePairs.Add(2, season.endDate);
                            break;
                        }
                    }

                return valuePairs;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception occurred in GetShowSeasonInformationAsyncQueryHandler");
                throw;
            }
        }
    }
}
