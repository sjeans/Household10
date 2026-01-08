using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.Shows.Queries;

public class GetShowDetails(int id) : IRequest<TVShowDto>
{
    public int Id { get; } = id;

    public class GetShowDetailsQueryHandler(IHttpClientFactory httpClientFactory, IMapper mapper, IAppJsonDeserializer jsonDeserializer) : IRequestHandler<GetShowDetails, TVShowDto>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;
        private IAppJsonDeserializer _jsonDeserializer = jsonDeserializer;

        public async Task<TVShowDto> Handle(GetShowDetails request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data!");

            TVShowDto showDetail = new();

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/tvshows/{request.Id}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    Result<TvShow> result = await _jsonDeserializer.TryDeserializeAsync<TvShow>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    TVShowDto? tvShow = _mapper.Map<TVShowDto>(result.Value);

                    if (tvShow is not null)
                        showDetail = tvShow;

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

            return showDetail;
        }
    }
}
