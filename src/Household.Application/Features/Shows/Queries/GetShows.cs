using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Shows.Queries;

public class GetShows() : IRequest<List<TVShowDto>>
{
    public class GetShowsQueryHandler(IHttpClientFactory httpClientFactory, IMapper mapper, IAppJsonDeserializer jsonDeserializer) : IRequestHandler<GetShows, List<TVShowDto>>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;
        private IAppJsonDeserializer _jsonDeserializer = jsonDeserializer;

        public async Task<List<TVShowDto>> Handle(GetShows request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client calls to retrieve data!");

            List<TVShowDto> shows = [];

            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/tvshows/", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = response.Content.ReadAsStream(cancellationToken);
                    Result<List<TvShow>> result = await _jsonDeserializer.TryDeserializeAsync<List<TvShow>>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    List<TVShowDto>? tvShows = _mapper.Map<List<TVShowDto>>(result.Value);

                    if (tvShows!.Count != 0)
                        shows = tvShows;

                }
            }
            catch (HttpRequestException hrex)
            {
                throw new BadRequestException(hrex.GetInnerMessage(), hrex);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.GetInnerMessage(), ex);
            }

            return shows;
        }
    }
}
