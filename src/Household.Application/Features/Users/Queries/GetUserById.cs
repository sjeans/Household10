using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;
using Serilog;

namespace Household.Application.Features.Users.Queries;

public class GetUserById(int id) : IRequest<User?>
{
    public int Id { get; } = id;

    public class GetUserByIdQueryHandler(ILogger Logger, IHttpClientFactory httpClientFactory, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetUserById, User?>
    {
        private readonly ILogger _logger = Logger;
        private HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<User?> Handle(GetUserById request, CancellationToken cancellationToken)
        {
            if (_client == null)
            {
                _logger.Error("HttpClient is null in GetUserByIdQueryHandler");
                throw new BadRequestException("Cannot make client call to retrieve data!");
            }

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/user/{request.Id}", cancellationToken);

                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<User> result = await _jsonDeserializer.TryDeserializeAsync<User>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                User? user = result.Value;

                if (user is not null)
                    return user;
                else
                {
                    _logger.Warning("No users found with Id: {UserId}", request.Id);
                    throw new NotFoundException("No users found!", user!);
                }
            }
            catch (HttpRequestException hrex)
            {
                _logger.Error(hrex, "HTTP request error in GetUserByIdQueryHandler for Id: {UserId}", request.Id);
                throw new BadRequestException(hrex.GetInnerMessage(), hrex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error in GetUserByIdQueryHandler for Id: {UserId}", request.Id);
                throw new BadRequestException(ex.Message);
            }
        }
    }
}
