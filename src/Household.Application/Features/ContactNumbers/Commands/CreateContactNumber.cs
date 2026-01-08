using System.Net;
using System.Net.Http.Json;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Models;
using MediatR;

namespace Household.Application.Features.ContactNumbers.Commands;

public class CreateContactNumber : IRequest<string>
{
    public ContactNumberDto NewContactNumber { get; }

    public CreateContactNumber(ContactNumberDto newContactNumber)
    {
        NewContactNumber = newContactNumber;
    }

    public class CreateContactNumberCommandHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<CreateContactNumber, string>
    {
        private HttpClient? _client = httpClientFactory.CreateClient("ApiClient");

        public async Task<string> Handle(CreateContactNumber request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannont make client calls to retrieve data!");

            if (request.NewContactNumber.Id > 0)
                throw new BadRequestException("Request must be a new contact number!");


            ContactNumber? contactNumber = new();

            contactNumber = new()
            {
                Id = request.NewContactNumber.Id,
                Name = request.NewContactNumber.Name,
                PhoneNumber = request.NewContactNumber.PhoneNumber,
            };

            try
            {
                if (contactNumber.Name == null || contactNumber.PhoneNumber == null)
                    throw new BadRequestException("Request is empty!");
                else
                {
                    HttpResponseMessage response = await _client.PostAsJsonAsync("api/contactnumber/", contactNumber, cancellationToken);
                    string responseString = await response.Content.ReadAsStringAsync();
                    HttpStatusCode statusCode = response.StatusCode;

                    if (response.IsSuccessStatusCode)
                        return response.StatusCode.ToString();
                    else
                        throw new BadRequestException("Could not create a new contact number!");

                }
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }
    }
}
