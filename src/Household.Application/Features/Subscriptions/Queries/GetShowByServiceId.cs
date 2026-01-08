using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Subscriptions.Queries;

public class GetShowByServiceId(int id) : IRequest<List<TVShowDto>>
{
    public int Id { get; } = id;

    public class GetShowByServiceIdQueryHandler(IHttpClientFactory httpClientFactory, IMapper mapper, IAppJsonDeserializer appJsonDeserializer) : IRequestHandler<GetShowByServiceId, List<TVShowDto>>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;
        private IAppJsonDeserializer _jsonDeserializer = appJsonDeserializer;

        public async Task<List<TVShowDto>> Handle(GetShowByServiceId request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new NotFoundException("Cannot make client call to retrieve data!");

            List<TVShowDto>? serviceShows = [];

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/subscription/shows/{request.Id}", cancellationToken);

                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<List<TvShow>> result = await _jsonDeserializer.TryDeserializeAsync<List<TvShow>>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                List<TVShowDto>? tvShows = _mapper.Map<List<TVShowDto>>(result.Value);

                if (tvShows != null && tvShows.Count > 0)
                    serviceShows = tvShows;
                else
                    throw new NotFoundException("Couldn't find shows for service id: {id}", request.Id);

            }
            catch (HttpRequestException hrex)
            {
                throw new BadRequestException(hrex.GetInnerMessage(), hrex);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
            }

            return serviceShows;
        }
    }
}
