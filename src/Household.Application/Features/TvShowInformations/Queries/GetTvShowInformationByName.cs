using System.Net.Http.Headers;
using System.Text;
using AutoMapper;
using Household.Shared.Models;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Services;

namespace Household.Application.Features.TvShowInformations.Queries;

public class GetTvShowInformationByName(string name) : IRequest<List<TVShowInformationDto>?>
{
    public string Name { get; } = name;

    public class GetTvShowInformationByNameQueryHandler(IHttpClientFactory httpClientFactory, IMapper mapper, IAppJsonDeserializer jsonDeserializer) : IRequestHandler<GetTvShowInformationByName, List<TVShowInformationDto>?>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;
        private readonly IAppJsonDeserializer _jsonDeserializer = jsonDeserializer;

        public async Task<List<TVShowInformationDto>?> Handle(GetTvShowInformationByName request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data!");

            List<TVShowInformationDto>? tvShowInformations = null;

            try
            {
                List<string> names = [request.Name];
                string content = JsonConvert.SerializeObject(names);
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                ByteArrayContent byteContent = new(buffer);

                HttpResponseMessage response = await _client.PostAsync("api/ShowInformation/GetAllShowInformation", byteContent, cancellationToken);

                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<List<TvShowInformation>> result = await _jsonDeserializer.TryDeserializeAsync<List<TvShowInformation>>(stream, cancellationToken);

                if(!result.IsSuccess)
                {
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                List<TVShowInformationDto> tVShows = _mapper.Map<List<TVShowInformationDto>>(result.Value);

                if (tVShows.Count > 0)
                    tvShowInformations = tVShows;
                //string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                //System.Net.HttpStatusCode statusCode = response.StatusCode;

                //if (response.IsSuccessStatusCode)
                //{
                //    List<TVShowInformationDto>? tvShows = _mapper.Map<List<TVShowInformationDto>>(JsonConvert.DeserializeObject<List<Shared.Models.TvShowInformation>>(responseString));

                //    if (tvShows!.Count != 0)
                //        tvShowInformations = tvShows;

                //}
            }
            catch (HttpRequestException hrex)
            {
                throw new BadRequestException(hrex.GetInnerMessage(), hrex);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.GetInnerMessage(), ex);
            }

            return tvShowInformations ?? null;
        }
    }
}
