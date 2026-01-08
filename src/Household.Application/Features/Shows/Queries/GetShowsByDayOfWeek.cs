using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace Household.Application.Features.Shows.Queries;

public class GetShowsByDayOfWeek(int dayOfWeek) : IRequest<List<TVShowDto>>
{
    public int Day { get; } = dayOfWeek;

    public class GetShowsByDayOfWeekQueryHandler(IHttpClientFactory httpClientFactory, IMapper mapper, IAppJsonDeserializer jsonDeserializer) : IRequestHandler<GetShowsByDayOfWeek, List<TVShowDto>>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;
        private readonly IAppJsonDeserializer _jsonDeserializer = jsonDeserializer;

        public async Task<List<TVShowDto>> Handle(GetShowsByDayOfWeek request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client calls to retrieve data!");

            List<TVShowDto> shows = [];

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/tvshows/dayofweek/{request.Day}", cancellationToken);

                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<List<TvShow>> result = await _jsonDeserializer.TryDeserializeAsync<List<TvShow>>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    // Handle error gracefully
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                List<TVShowDto>? tvShows = _mapper.Map<List<TVShowDto>>(result.Value);
                if (tvShows!.Count != 0)
                    shows = tvShows;

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
