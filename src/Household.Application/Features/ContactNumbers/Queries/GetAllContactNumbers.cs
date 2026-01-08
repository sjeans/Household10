using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.ContactNumbers.Queries;

public class GetAllContactNumbers : IRequest<List<ContactNumberDto>>
{
    public GetAllContactNumbers() { }

    public class GetAllContactNumbersQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetAllContactNumbers, List<ContactNumberDto>>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<List<ContactNumberDto>> Handle(GetAllContactNumbers request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data!");

            List<ContactNumberDto> contactNumbers = [];

            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/contactnumber/", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = response.Content.ReadAsStream(cancellationToken);
                    Result<List<ContactNumber>> result = await _jsonDeserializer.TryDeserializeAsync<List<ContactNumber>>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    List<ContactNumber>? contactList = result.Value;

                    contactList?.ForEach(contact =>
                    {
                        contactNumbers.Add(new ()
                        {
                            Id = contact.Id,
                            Name = contact.Name,
                            PhoneNumber = contact.PhoneNumber,
                        });
                    });
                        
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

            return contactNumbers;
        }
    }
}
