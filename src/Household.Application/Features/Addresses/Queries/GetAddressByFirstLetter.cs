using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Addresses.Queries;

public class GetAddressByFirstLetter(string letter) : IRequest<List<AddressInfoDto>>
{
    public string Letter { get; } = letter.ToLower();

    public class GetAddressByFirstLetterQueryHandler(IHttpClientFactory httpClientFactory, IMapper mapper, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetAddressByFirstLetter, List<AddressInfoDto>?>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<List<AddressInfoDto>?> Handle(GetAddressByFirstLetter request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new NotFoundException("Cannot make client call to retrieve data!");

            List<AddressInfoDto> addressInfo = [];

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/addressbook/{request.Letter}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = response.Content.ReadAsStream(cancellationToken);
                    Result<List<AddressInfo>> result = await _jsonDeserializer.TryDeserializeAsync<List<AddressInfo>>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    List<AddressInfoDto>? address = _mapper.Map<List<AddressInfoDto>?>(result.Value);

                    if (address is not null)
                        addressInfo = address;

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
