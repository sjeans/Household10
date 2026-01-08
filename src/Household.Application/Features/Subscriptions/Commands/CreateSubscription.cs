using System.Net.Http.Json;
using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using MediatR;

namespace Household.Application.Features.Subscriptions.Commands;

public class CreateSubscription(StreamingServiceDto newSubscription) : IRequest<string>
{
    public StreamingServiceDto NewSubscription { get; } = newSubscription;

    public class CreateSubscriptionCommandHandler(IHttpClientFactory httpClientFactory, IMapper mapper) : IRequestHandler<CreateSubscription, string>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;

        public async Task<string> Handle(CreateSubscription request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new NotFoundException("Cannot retrieve data!");

            if (request.NewSubscription.Id > 0)
                throw new BadRequestException("Request must be a new streaming service!");

            StreamingService streamingService = _mapper.Map<StreamingService>(request.NewSubscription);

            try
            {
                HttpResponseMessage response = await _client.PostAsJsonAsync("api/subscription/", streamingService, cancellationToken);
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
