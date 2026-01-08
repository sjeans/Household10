using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Users.Queries;

public class GetAllUsers : IRequest<List<User>>
{
    public GetAllUsers()
    {
    }

    public class GetAllUsersQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetAllUsers, List<User>>
    {
        private HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<List<User>> Handle(GetAllUsers request, CancellationToken cancellationToken)
        {

            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data!");

            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/user/", cancellationToken);

                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<List<User>> result = await _jsonDeserializer.TryDeserializeAsync<List<User>>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                List<User>? usersList = result.Value;

                if (usersList != null)
                    return usersList;
                else
                    throw new NotFoundException("No users found!", usersList!);

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
