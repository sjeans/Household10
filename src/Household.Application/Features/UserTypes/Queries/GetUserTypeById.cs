using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.UserTypes.Queries;

public class GetUserTypeById(int id) : IRequest<UserType?>
{
    public int Id { get; } = id;

    public class GetUserTypeByIdQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetUserTypeById, UserType?>
    {
        private HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<UserType?> Handle(GetUserTypeById request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data!");

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/usertype/{request.Id}", cancellationToken);

                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<UserType> result = await _jsonDeserializer.TryDeserializeAsync<UserType>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                UserType? userType = result.Value;

                if (userType != null)
                    return userType;
                else
                    throw new NotFoundException("No users found!", userType!);

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
