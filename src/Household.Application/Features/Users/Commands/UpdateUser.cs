using System.Net;
using System.Net.Http.Headers;
using System.Text;
using AutoMapper;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using MediatR;
using Newtonsoft.Json;

namespace Household.Application.Features.Users.Commands;

public class UpdateUser(User updateUser) : IRequest<string>
{
    public User UpdatedUser { get; } = updateUser;

    public class UpdateUserHandler(IHttpClientFactory httpClientFactory, IMapper mapper) : IRequestHandler<UpdateUser, string>
    {
        private HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private IMapper _mapper = mapper;

        public async Task<string> Handle(UpdateUser request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data.");

            if (request.UpdatedUser.Id <= 0)
                throw new BadRequestException("Cannot be a new user!");

            User userDetail = _mapper.Map<User>(request.UpdatedUser);

            try
            {
                string content = JsonConvert.SerializeObject(userDetail);
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                ByteArrayContent byteContent = new ByteArrayContent(buffer);

                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await _client.PutAsync($"api/user/{request.UpdatedUser.Id}", byteContent, cancellationToken);
                string responseString = await response.Content.ReadAsStringAsync();
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
