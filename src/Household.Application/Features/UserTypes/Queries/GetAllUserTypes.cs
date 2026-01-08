using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.UserTypes.Queries;

public class GetAllUserTypes : IRequest<List<UserType>>
{
    public GetAllUserTypes()
    {
    }

    public class GetAllUserTypesQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetAllUserTypes, List<UserType>>
    {
        private HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<List<UserType>> Handle(GetAllUserTypes request, CancellationToken cancellationToken)
        {

            if (_client == null)
            {
                throw new BadRequestException("Cannot make client call to retrieve data!");
            }

            List<UserType>? userTypes = new();

            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/usertype", cancellationToken);

                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<List<UserType>> result = await _jsonDeserializer.TryDeserializeAsync<List<UserType>>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                userTypes = result.Value;

                if (userTypes == null)
                {
                    throw new NotFoundException("User types not found!", userTypes!);
                }
            }
            catch (HttpRequestException hrex)
            {
                throw new BadRequestException(hrex.GetInnerMessage(), hrex);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
            }

            return userTypes;
        }
    }
}
