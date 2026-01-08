using System.Net;
using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Shows.Commands;

public class DeleteShow(int id) : IRequest<string>
{
    private int Id { get; } = id;

    public class DeleteTvShowHandler(IHttpClientFactory httpClientFactory, IMapper mapper, IAppJsonDeserializer jsonDeserializer) : IRequestHandler<DeleteShow, string>
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;
        private IAppJsonDeserializer _jsonDeserializer = jsonDeserializer;

        public async Task<string> Handle(DeleteShow request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannont make client call to retrieve data!");

            if (request.Id <= 0)
                throw new BadRequestException("Request must be an existing show!");

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/tvshows/{request.Id}");

                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<TvShow> result = await _jsonDeserializer.TryDeserializeAsync<TvShow>(stream, cancellationToken);

                if(!result.IsSuccess)
                {
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                TVShowDto? existingShow = _mapper.Map<TVShowDto?>(result.Value);

                if (existingShow != null)
                {
                    response = await _client.DeleteAsync($"api/tvshows/{existingShow?.Id}", cancellationToken);
                    string responseString = await response.Content.ReadAsStringAsync();
                    HttpStatusCode statusCode = response.StatusCode;
                    if (response.IsSuccessStatusCode)
                        return response.StatusCode.ToString();
                    else
                        throw new BadRequestException(responseString);

                }
                else
                {
                    throw new BadRequestException($"Could not find tv show id: {request.Id}");
                }
            }
            catch (HttpRequestException hrex)
            {
                throw new BadRequestException(hrex.GetInnerMessage(), hrex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetInnerMessage());
                throw new BadRequestException(ex.GetInnerMessage(), ex);
            }
        }
    }
}
