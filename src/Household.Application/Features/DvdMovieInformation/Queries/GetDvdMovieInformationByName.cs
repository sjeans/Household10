using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using MediatR;
using Dvd = Household.Shared.Models;

namespace Household.Application.Features.DvdMovieInformation.Queries;

public class GetDvdMovieInformationByName(string name) : IRequest<Dvd.DvdMovieInformation?>
{
    public string Name { get; } = name;

    public class GetDvdMovieByNameQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetDvdMovieInformationByName, Dvd.DvdMovieInformation?>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<Dvd.DvdMovieInformation?> Handle(GetDvdMovieInformationByName request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client calls to retrieve data!");

            Dvd.DvdMovieInformation? movieInfo = null;

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/dvdmovieinformation/title/{request.Name}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    Result<Dvd.DvdMovieInformation> result = await _jsonDeserializer.TryDeserializeAsync<Dvd.DvdMovieInformation>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    movieInfo = result.Value;
                }
            }
            catch (OperationCanceledException ocex)
            {
                throw new BadRequestException(ocex.GetInnerMessage(), ocex);
            }
            catch (HttpRequestException hrex)
            {
                throw new BadRequestException(hrex.GetInnerMessage(), hrex);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.GetInnerMessage(), ex);
            }

            return movieInfo;
        }
    }
}
