using System.Net;
using System.Net.Http.Headers;
using System.Text;
using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using MediatR;
using Newtonsoft.Json;

namespace Household.Application.Features.Subscriptions.Commands;

public class UpdateSubscription(StreamingServiceDto updateSubscription) : IRequest<string>
{
    public StreamingServiceDto UpdatedSubscription { get; } = updateSubscription;

    public class UpdateSubscriptionCommandHandler(IHttpClientFactory httpClientFactory, IMapper mapper) : IRequestHandler<UpdateSubscription, string>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;

        public async Task<string> Handle(UpdateSubscription request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data.");

            if (request.UpdatedSubscription.Id <= 0)
                throw new BadRequestException("Cannot be a new streaming service!");

            StreamingService serviceDetail = _mapper.Map<StreamingService>(request.UpdatedSubscription);

            try
            {
                string content = JsonConvert.SerializeObject(serviceDetail);
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                ByteArrayContent byteContent = new (buffer);

                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await _client.PutAsync($"api/subscription/{request.UpdatedSubscription.Id}", byteContent, cancellationToken);
                string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                HttpStatusCode statusCode = response.StatusCode;

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
