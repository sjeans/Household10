using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.ContactNumbers.Queries;

public class GetContactById(int id) : IRequest<ContactNumberDto>
{
    public int Id { get; } = id;

    public class GetContactByIdQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetContactById, ContactNumberDto>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<ContactNumberDto> Handle(GetContactById request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data!");

            ContactNumberDto contactNumber = new();

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/contactnumber/{request.Id}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    Result<ContactNumber> result = await _jsonDeserializer.TryDeserializeAsync<ContactNumber>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    ContactNumber? contact = result.Value;

                    if (contact is not null)
                        contactNumber = new ()
                        {
                            Id = contact.Id,
                            Name = contact.Name,
                            PhoneNumber = contact.PhoneNumber,
                        };

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

            return contactNumber;
        }
    }
}
