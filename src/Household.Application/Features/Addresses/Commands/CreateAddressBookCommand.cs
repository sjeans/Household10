using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Ardalis.GuardClauses;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using MediatR;
using Newtonsoft.Json;

namespace Household.Application.Features.Addresses.Commands;

public class CreateAddressBookCommand(AddressInfoDto newAddress) : IRequest<string>
{
    public AddressInfoDto NewAddress { get; } = newAddress;

    public class CreateAddressBookCommandHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<CreateAddressBookCommand, string>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");

        public async Task<string> Handle(CreateAddressBookCommand request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannont retrieve data!");

            if (request.NewAddress.Id > 0)
                throw new BadRequestException("Request must be a new addressbook entry!");

            AddressInfoDto? addressDetail = new()
            {
                Id = Convert.ToInt32(Guard.Against.NullOrEmpty(request.NewAddress.Id.ToString())),
                Address = Guard.Against.NullOrEmpty(request.NewAddress.Address),
                Address2 = request.NewAddress.Address2,
                City = request.NewAddress.City,
                CountryCode = request.NewAddress.CountryCode,
                Name = Guard.Against.NullOrEmpty(request.NewAddress.Name),
                PostalCode = request.NewAddress.PostalCode,
                State = request.NewAddress.State,
            };

            try
            {
                HttpResponseMessage response = await _client.PostAsJsonAsync("api/addressbook/", addressDetail, cancellationToken);
                string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                    throw new BadRequestException("Could not add new addressbook entry!");
                else
                    return response.StatusCode.ToString();

            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }
    }
}
