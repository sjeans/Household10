using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Subscriptions.Queries;

public class GetAllSubscriptions() : IRequest<List<StreamingServiceDto>>
{
    public class GetAllSubscriptionsQueryHandler(IHttpClientFactory httpClientFactory, IMapper mapper, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetAllSubscriptions, List<StreamingServiceDto>>
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<List<StreamingServiceDto>> Handle(GetAllSubscriptions request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new NotFoundException("Cannot make client call to retrieve data!");

            List<StreamingServiceDto> service = [];

            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/subscription", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    Result<List<StreamingService>> result = await _jsonDeserializer.TryDeserializeAsync<List<StreamingService>>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    List<StreamingServiceDto>? streamingService = _mapper.Map<List<StreamingServiceDto>?>(result.Value);

                    if (streamingService!.Count != 0)
                        service = streamingService;

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

            return service;
        }
    }
}
