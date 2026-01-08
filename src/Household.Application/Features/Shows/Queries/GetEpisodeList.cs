using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Shows.Queries;

public class GetEpisodeList : IRequest<List<int>?>
{
    public class GetEpisodeListQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer jsonDeserializer) : IRequestHandler<GetEpisodeList, List<int>?>
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient("ApiClient");
        private IAppJsonDeserializer _jsonDeserializer = jsonDeserializer;

        public async Task<List<int>?> Handle(GetEpisodeList request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data!");

            try
            {
                List<int>? episodes = [];

                HttpRequestMessage requestEpisodeList = new()
                {
                    RequestUri = new Uri("api/tvshows/episodes"),
                    Method = HttpMethod.Get
                };

                HttpResponseMessage response = await _client.SendAsync(requestEpisodeList, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    Result<List<int>> result = await _jsonDeserializer.TryDeserializeAsync<List<int>>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    //API call success, Do your action
                    episodes = result.Value;
                    if (episodes != null)
                        return episodes;

                }
                else
                    throw new BadRequestException("Parsing error.");

                throw new NotFoundException("Nothing to return.");
            }
            catch (HttpRequestException hrex)
            {
                throw new BadRequestException(hrex.GetInnerMessage(), hrex);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }
    }
}
