using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Addresses.Queries;

public class GetAllAddresses() : IRequest<List<AddressInfoDto>>
{
    public class GetAllAddressesQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetAllAddresses, List<AddressInfoDto>>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<List<AddressInfoDto>> Handle(GetAllAddresses request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client calls to retrieve data!");

            List<AddressInfoDto> addressInfos = [];

            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/addressbook", cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    Result<List<AddressInfo>> result = await _jsonDeserializer.TryDeserializeAsync<List<AddressInfo>>(stream, cancellationToken);

                    if(!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    List<AddressInfo>? addressList = result.Value;

                    foreach(AddressInfo address in addressList!)
                    {
                        if (address.ContactNumbers!.Count > 0)
                        {
                            List<ContactNumberDto> contactNumberDto = [];
                            foreach (ContactNumber contactNumber in address.ContactNumbers!)
                            {
                                contactNumberDto.Add(new ()
                                {
                                    Id = contactNumber.Id,
                                    Name = contactNumber.Name,
                                    PhoneNumber = contactNumber.PhoneNumber,
                                });
                            }

                            addressInfos.Add(new AddressInfoDto()
                            {
                                Id = address.Id,
                                Address = address.Address,
                                Address2 = address.Address2,
                                City = address.City,
                                CountryCode = address.CountryCode,
                                Name = address.Name,
                                PostalCode = address.PostalCode,
                                State = address.State,
                                ContactNumbers = contactNumberDto,
                            });
                        }
                        else
                        {
                            addressInfos.Add(new AddressInfoDto()
                            {
                                Id = address.Id,
                                Address = address.Address,
                                Address2 = address.Address2,
                                City = address.City,
                                CountryCode = address.CountryCode,
                                Name = address.Name,
                                PostalCode = address.PostalCode,
                                State = address.State,
                            });
                        }
                    }
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

            return addressInfos;
        }
    }
}
