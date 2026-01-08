using System.Net.Http.Json;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using MediatR;

namespace Household.Application.Features.Episodes.Commands;

public class AddEpisodeInformationAsync(int id, Dictionary<string, int> valuePairs) : IRequest<string>
{
    public int Id { get; internal set; } = id;
    public Dictionary<string, int> ValuePairs { get; internal set; } = valuePairs;

    public class AddEpisodeInformationAsyncCommandHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<AddEpisodeInformationAsync, string>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");

        public async Task<string> Handle(AddEpisodeInformationAsync request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new NotFoundException("Cannot make client call to retrieve data!");

            if (request.Id == 0)
                throw new BadRequestException("Request must be a new tv show!");

            try
            {
                HttpResponseMessage response = await _client.PutAsJsonAsync($"api/episodedetail/addepisodetoshow/{request.Id}", request.ValuePairs, cancellationToken);
                string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                    return response.StatusCode.ToString();
                else
                    throw new BadRequestException(responseString);

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
