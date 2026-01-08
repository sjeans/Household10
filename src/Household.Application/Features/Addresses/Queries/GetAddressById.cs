using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Addresses.Queries;

public class GetAddressById(int id) : IRequest<AddressInfoDto>
{
    public int Id { get; } = id;

    public class GetAddressByIdQueryHandler(IHttpClientFactory httpClientFactory, IMapper mapper, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetAddressById, AddressInfoDto?>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<AddressInfoDto?> Handle(GetAddressById request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new NotFoundException("Cannot make client call to retrieve data!");

            AddressInfoDto addressInfo = new();

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/addressbook/{request.Id}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = response.Content.ReadAsStream(cancellationToken);
                    Result<AddressInfo> result = await _jsonDeserializer.TryDeserializeAsync<AddressInfo>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    AddressInfoDto? address = _mapper.Map<AddressInfoDto?>(result.Value);

                    if (address is not null)
                        addressInfo = address;
                    else
                        throw new NotFoundException("Address not found!");

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

            return addressInfo;
        }
    }
}
