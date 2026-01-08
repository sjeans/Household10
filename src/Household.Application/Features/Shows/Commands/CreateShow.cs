using System.Net.Http.Json;
using AutoMapper;
using Household.Application.Features.TvShowInformations.Queries;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using MediatR;

namespace Household.Application.Features.Shows.Commands;

public class CreateShow(TVShowDto newShow, IMediator mediator) : IRequest<string>
{
    public TVShowDto NewShow { get; } = newShow;
    public IMediator Mediator { get; } = mediator;

    public class CreateShowCommandHandler(IHttpClientFactory httpClientFactory, IMapper mapper) : IRequestHandler<CreateShow, string>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;

        public async Task<string> Handle(CreateShow request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data!");

            if (request.NewShow.Id > 0)
                throw new BadRequestException("Request must be a new show!");

            TvShow? tvShow = _mapper.Map<TvShow>(request.NewShow);
            if(tvShow != null)
                tvShow.StreamingServiceId = request.NewShow.StreamingId;

            try
            {
                HttpResponseMessage response = await _client.PostAsJsonAsync("api/tvshows/", tvShow, cancellationToken);
                string responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TvShowInformation? tvShowInfo = await request.Mediator.Send(new RetrieveTvShowInformationByName(request.NewShow.Name, true), cancellationToken);

                    if(tvShowInfo is not null)
                        return response.StatusCode.ToString();

                    return response.StatusCode.ToString();
                }
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
