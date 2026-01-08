using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.MovieInformation.Queries;

public class GetAllMovieInformation : IRequest<List<MovieInfoDto>>
{
    public GetAllMovieInformation()
    {
    }

    public class GetAllMovieInformationQueryHandler : IRequestHandler<GetAllMovieInformation, List<MovieInfoDto>>
    {
        private HttpClient? _client;
        private IAppJsonDeserializer _jsonDeserializer;

        public GetAllMovieInformationQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer jsonDeserializer)
        {
            _client = httpClientFactory.CreateClient("ApiClient");
            _jsonDeserializer = jsonDeserializer;
        }

        public async Task<List<MovieInfoDto>> Handle(GetAllMovieInformation request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data!");

            List<MovieInfoDto> movieInfo = new List<MovieInfoDto>();

            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/movieinfo", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await using Stream stream = response.Content.ReadAsStream(cancellationToken);
                    Result<List<MovieInfo>> result = await _jsonDeserializer.TryDeserializeAsync<List<MovieInfo>>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    List<MovieInfo>? movies = result.Value;

                    if (movies!.Any())
                    {
                        movies?.ForEach(movie =>
                        {
                            movieInfo.Add(new MovieInfoDto()
                            {
                                CheckedoutTo = movie.CheckedoutTo,
                                Collectible = Convert.ToBoolean(movie.MvCollectible),
                                Description = movie.Description ?? string.Empty,
                                DiskNum = movie.MvDiskNum,
                                DownloadDate = movie.DownloadDate,
                                Downloaded = Convert.ToBoolean(movie.Downloaded),
                                ExpirationDate = movie.ExpirationDate,
                                FirstName = movie.FirstName,
                                HasDownload = Convert.ToBoolean(movie.HasDownload),
                                Id = movie.MvId,
                                Is3D = Convert.ToBoolean(movie.Mv3D),
                                Is4K = Convert.ToBoolean(movie.Mv4K),
                                LastName = movie.LastName,
                                Name = movie.Name,
                                Title = movie.MvTitle,
                            });
                        });
                    }
                }
                else
                    throw new NotFoundException("No shows found!", movieInfo);

            }
            catch (HttpRequestException hrex)
            {
                throw new BadRequestException(hrex.GetInnerMessage(), hrex);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
            }

            return movieInfo;
        }
    }
}
