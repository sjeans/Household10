using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using MediatR;
using Newtonsoft.Json;

namespace Household.Application.Features.Addresses.Commands;

public class UpdateAddressBookCommand(AddressInfoDto addressInfo) : IRequest<string>
{
    public readonly AddressInfoDto UpdatedAddress = addressInfo;

    public class UpdateAddressBookCommandHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<UpdateAddressBookCommand, string>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");

        public async Task<string> Handle(UpdateAddressBookCommand request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot update data!");

            if (request.UpdatedAddress.Id <= 0)
                throw new BadRequestException("Cannot be a new show!");

            AddressInfoDto? addressDetail = new()
            {
                Id = request.UpdatedAddress.Id,
                Address = request.UpdatedAddress.Address,
                Address2 = request.UpdatedAddress.Address2,
                City = request.UpdatedAddress.City,
                CountryCode = request.UpdatedAddress.CountryCode,
                Name = request.UpdatedAddress.Name,
                PostalCode = request.UpdatedAddress.PostalCode,
                State = request.UpdatedAddress.State,
            };

            try
            {
                string content = JsonConvert.SerializeObject(addressDetail);
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                ByteArrayContent byteContent = new (buffer);

                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await _client.PutAsync($"api/addressbook/{request.UpdatedAddress.Id}", byteContent, cancellationToken);
                string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                HttpStatusCode statusCode = response.StatusCode;

                if (response.IsSuccessStatusCode)
                    return response.StatusCode.ToString();
                else
                    throw new BadRequestException(responseString);

            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }
    }
}
