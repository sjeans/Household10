using System.Net.Http.Json;
using AutoMapper;
using Household.Shared.Dtos.Exceptions;
using MediatR;

namespace Household.Application.Features.TvShowInformations.Commands;

public class UpdateTvShowInformationAsync(int id, Dictionary<string, int> valuePairs) : IRequest<string>
{
    public int Id { get; internal set; } = id;
    public Dictionary<string, int> ValuePairs { get; internal set; } = valuePairs;

    public class UpdateTvShowInformationAsyncCommandHandler(IHttpClientFactory httpClientFactory, IMapper mapper) : IRequestHandler<UpdateTvShowInformationAsync, string>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;

        public async Task<string> Handle(UpdateTvShowInformationAsync request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new NotFoundException("Cannot make client call to retrieve data!");

            if (request.Id == 0)
                throw new BadRequestException("Request must be a new tv show!");

            try
            {
                HttpResponseMessage response = await _client.PutAsJsonAsync($"api/showinformation/{request.Id}", request.ValuePairs, cancellationToken);
                string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                    return response.StatusCode.ToString();
                else
                    throw new BadRequestException(responseString);

            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }
    }
}
