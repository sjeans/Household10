using System.Net.Http.Json;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Models;
using MediatR;

namespace Household.Application.Features.TvShowInformations.Commands;

public class CreateTvShowInformationAsync(TvShowInformation showInformation) : IRequest<string>
{
    public TvShowInformation ShowInformation { get; } = showInformation;
    public int Id { get; }

    public class CreateTvShowInformationAsyncCommandHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<CreateTvShowInformationAsync, string>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");

        public async Task<string> Handle(CreateTvShowInformationAsync request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new NotFoundException("Cannot make client call to retrieve data!");

            if (request.ShowInformation.Id > 0)
                throw new BadRequestException("Request must be a new tv show!");

            try
            {
                HttpResponseMessage response = await _client.PostAsJsonAsync("api/showinformation/", request.ShowInformation, cancellationToken);
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
