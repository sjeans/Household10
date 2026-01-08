using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services;
using Household.Shared.Services.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Household.Application.Features.TvShowInformations.Queries;

public class RetrieveTvShowInformationByName(string name, bool createNew = false) : IRequest<TvShowInformation?>
{
    public string Name { get; } = name;
    private bool CreateNew { get; } = createNew;

    public class RetrieveTvShowInformationByNameQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer jsonDeserializer) : IRequestHandler<RetrieveTvShowInformationByName, TvShowInformation?>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IAppJsonDeserializer _jsonDeserializer = jsonDeserializer;

        public async Task<TvShowInformation?> Handle(RetrieveTvShowInformationByName request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot retrieve data!");

            TvShowInformation? tvShowInformation = null;

            try
            {
                string content = JsonConvert.SerializeObject(request.Name);
                //byte[] buffer = Encoding.UTF8.GetBytes(content);
                //ByteArrayContent byteContent = new(buffer);

                //byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await _client.PostAsJsonAsync($"api/ShowInformation/RetrieveShowDetails/{request.CreateNew}", request.Name, cancellationToken);


                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<TvShowInformation> result = await _jsonDeserializer.TryDeserializeAsync<TvShowInformation>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    // Handle error gracefully
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                TvShowInformation tvShows = result.Value!;

                if (tvShows is not null)
                    tvShowInformation = tvShows;

            }
            catch (HttpRequestException hrex)
            {
                throw new BadRequestException(hrex.GetInnerMessage(), hrex);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.GetInnerMessage(), ex);
            }

            return tvShowInformation ?? null;
        }
    }
}
