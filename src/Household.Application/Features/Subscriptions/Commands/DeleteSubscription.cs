using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Subscriptions.Commands;

public class DeleteSubscription(int id) : IRequest<string>
{
    private int Id { get; } = id;

    public class DeleteSubscriptionHandler(IHttpClientFactory httpClientFactory, IMapper mapper, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<DeleteSubscription, string>
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<string> Handle(DeleteSubscription request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannont make client call to retrieve data!");

            if (request.Id <= 0)
                throw new BadRequestException("Request must be an existing subscription!");

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/subscription/{request.Id}");

                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<StreamingServiceDto> result = await _jsonDeserializer.TryDeserializeAsync<StreamingServiceDto>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                StreamingServiceDto? existingSubscription = _mapper.Map<StreamingServiceDto?>(result.Value);

                if (existingSubscription != null)
                {
                    response = await _client.DeleteAsync($"api/subscription/{existingSubscription?.Id}", cancellationToken);
                    
                    return response.StatusCode.ToString();
                }
                else
                {
                    throw new BadRequestException($"Could not find subscription id: {request.Id}");
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
        }
    }
}
