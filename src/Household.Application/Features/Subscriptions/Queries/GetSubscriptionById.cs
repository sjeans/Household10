using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Subscriptions.Queries;

public class GetSubscriptionById(int id) : IRequest<StreamingServiceDto>
{
    public int Id { get; } = id;

    public class GetSubscriptionByIdQueryHandler(IHttpClientFactory httpClientFactory, IMapper mapper, IAppJsonDeserializer jsonDeserializer) : IRequestHandler<GetSubscriptionById, StreamingServiceDto>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;
        private IAppJsonDeserializer _jsonDeserializer = jsonDeserializer;

        public async Task<StreamingServiceDto> Handle(GetSubscriptionById request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data!");

            StreamingServiceDto service = new();

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/subscription/{request.Id}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    Result<StreamingServiceDto> result = await _jsonDeserializer.TryDeserializeAsync<StreamingServiceDto>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    StreamingServiceDto? streamingService = _mapper.Map<StreamingServiceDto>(result.Value);

                    if (streamingService is not null)
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
