using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.TvShowInformations.Queries;

public class GetTvShowInformationNames() : IRequest<Dictionary<int, string>?>
{
    public class GetTvShowInformationNamesQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer jsonDeserializer) : IRequestHandler<GetTvShowInformationNames, Dictionary<int, string>?>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IAppJsonDeserializer _jsonDeserializer = jsonDeserializer;

        public async Task<Dictionary<int, string>?> Handle(GetTvShowInformationNames request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot retrieve data!");

            Dictionary<int, string>? tvShowNames = null;

            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/showinformation/names", cancellationToken);

                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<Dictionary<int, string>> result = await _jsonDeserializer.TryDeserializeAsync<Dictionary<int, string>>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    // Handle error gracefully
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                Dictionary<int, string>? tvNames = result.Value;

                if (tvNames!.Count != 0)
                    tvShowNames = tvNames;

            }
            catch (HttpRequestException hrex)
            {
                throw new BadRequestException(hrex.GetInnerMessage(), hrex);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.GetInnerMessage(), ex);
            }

            return tvShowNames;
        }
    }
}
