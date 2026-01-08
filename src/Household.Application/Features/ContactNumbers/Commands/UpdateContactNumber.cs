using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using MediatR;
using Newtonsoft.Json;

namespace Household.Application.Features.ContactNumbers.Commands;

public class UpdateContactNumber : IRequest<string>
{
    public ContactNumberDto UpdatedContactNumber { get; }

    public UpdateContactNumber(ContactNumberDto updatedContactNumber) 
    {
        UpdatedContactNumber = updatedContactNumber;
    }

    public class UpdateContactNumberCommandHandler : IRequestHandler<UpdateContactNumber, string>
    {
        private HttpClient? _client;

        public UpdateContactNumberCommandHandler(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<string> Handle(UpdateContactNumber request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to update data!");

            if (request.UpdatedContactNumber.Id <= 0)
                throw new BadRequestException("Cannot be a new show!");

            ContactNumber? contactDetail = new()
            {
                Id = request.UpdatedContactNumber.Id,
                Name = request.UpdatedContactNumber.Name,
                PhoneNumber = request.UpdatedContactNumber.PhoneNumber,
            };

            try
            {
                string content = JsonConvert.SerializeObject(contactDetail);
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                ByteArrayContent byteContent = new ByteArrayContent(buffer);

                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await _client.PutAsync($"api/contactnumber/{request.UpdatedContactNumber.Id}", byteContent, cancellationToken);
                string responseString = await response.Content.ReadAsStringAsync();
                HttpStatusCode statusCode = response.StatusCode;

                if (response.IsSuccessStatusCode)
                    return response.StatusCode.ToString();
                else
                    throw new NotFoundException("Could not update contact number! ");

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
