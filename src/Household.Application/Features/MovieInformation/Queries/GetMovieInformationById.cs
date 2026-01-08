using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Enums;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.MovieInformation.Queries;

public class GetMovieInformationById(int id) : IRequest<MovieInfoDto>
{
    public int Id { get; } = id;

    public class GetMovieInformationByIdQueryHandler(IHttpClientFactory httpClientFactory, IAppJsonDeserializer jsonDeserializer) : IRequestHandler<GetMovieInformationById, MovieInfoDto>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private IAppJsonDeserializer _jsonDeserializer = jsonDeserializer;

        public async Task<MovieInfoDto> Handle(GetMovieInformationById request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to retrieve data!");

            MovieInfoDto movieInfo = new();

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/movieinfo/{request.Id}", cancellationToken);

                if(response.IsSuccessStatusCode)
                {
                    await using Stream stream = response.Content.ReadAsStream(cancellationToken);
                    Result<MovieInfo> result = await _jsonDeserializer.TryDeserializeAsync<MovieInfo>(stream, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        throw new BadRequestException($"Failed to deserialize: {result.Error}");
                    }

                    MovieInfo? movie = result.Value;
                
                    if (movie != null)
                    {
                        //DvdTypeDto? dvdType = await dvdClient.GetFromJsonAsync<DvdTypeDto>($"api/DiskType/{movie.Dtid}", cancellationToken);
                        CheckedOut checkedOut = await GetCheckedoutAsync(movie, cancellationToken);
                        DigitalDownload digitalDownload = await GetDigitalDownloadAsync(movie, cancellationToken);
                        User user = await GetUserAsync(movie, cancellationToken);

                        if (digitalDownload?.MvId == 0)
                            digitalDownload.MvId = movie.MvId;

                        if (movie?.Downloaded == null)
                            movie!.Downloaded = false;

                        if (movie?.HasDownload == null)
                            movie!.HasDownload = false;

                        movieInfo = new MovieInfoDto()
                        {
                            CheckedoutTo = movie.CheckedoutTo,
                            Collectible = movie.MvCollectible,
                            Checkout = checkedOut,
                            DigitalDownload = digitalDownload,
                            Description = movie.Description ?? string.Empty,
                            DiskNum = movie.MvDiskNum,
                            DownloadDate = movie.DownloadDate,
                            Downloaded = Convert.ToBoolean(movie.Downloaded),
                            //DvdType = dvdType ?? new(),
                            DvdType = (DvdTypes)(movie.Dtid ?? 0),
                            ExpirationDate = movie.ExpirationDate,
                            FirstName = movie.FirstName,
                            HasDownload = Convert.ToBoolean(movie.HasDownload),
                            Id = movie.MvId,
                            Is3D = movie.Mv3D,
                            Is4K = movie.Mv4K,
                            LastName = movie.LastName,
                            Name = movie.Name,
                            Title = movie.MvTitle,
                            UserInfo = user,
                        };
                    }
                    else
                        throw new NotFoundException("Move not found!");

                }
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

        private async Task<CheckedOut> GetCheckedoutAsync(MovieInfo movie, CancellationToken cancellationToken)
        {
            CheckedOut checkedOut = new();

            if (movie.Coid is not null)
            {
                HttpResponseMessage response = await _client!.GetAsync($"api/checkedout/{movie.Coid}", cancellationToken);

                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<CheckedOut> result = await _jsonDeserializer.TryDeserializeAsync<CheckedOut>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                checkedOut = result.Value!;
            }

            return checkedOut;
        }

        private async Task<DigitalDownload> GetDigitalDownloadAsync(MovieInfo movie, CancellationToken cancellationToken)
        {
            DigitalDownload digitalDownload = new();

            if (movie.Ddid is not null)
            {
                HttpResponseMessage response = await _client!.GetAsync($"api/digitaldownload/{movie.Ddid}");

                response.EnsureSuccessStatusCode();

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                Result<DigitalDownload> result = await _jsonDeserializer.TryDeserializeAsync<DigitalDownload>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                digitalDownload = result.Value!;
            }

            return digitalDownload;
        }

        private async Task<User> GetUserAsync(MovieInfo movie, CancellationToken cancellationToken)
        {
            UserMovie userMovie = await GetUserMovieAsync(movie, cancellationToken);
            User user = new();

            if (userMovie.UserId != 0)
            {
                HttpResponseMessage response = await _client!.GetAsync($"api/user/{userMovie.UserId}");

                response.EnsureSuccessStatusCode();

                await using Stream stream = response.Content.ReadAsStream(cancellationToken);
                Result<User> result = await _jsonDeserializer.TryDeserializeAsync<User>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                user = result.Value!;
            }

            return user;
        }

        private async Task<UserMovie> GetUserMovieAsync(MovieInfo movie, CancellationToken cancellationToken)
        {
            List<UserMovie> userMovies = new();

            if (movie.MvId != 0)
            {
                HttpResponseMessage response = await _client!.GetAsync("api/usermovie");

                response.EnsureSuccessStatusCode();

                await using Stream stream = response.Content.ReadAsStream(cancellationToken);
                Result<List<UserMovie>> result = await _jsonDeserializer.TryDeserializeAsync<List<UserMovie>>(stream, cancellationToken);

                if (!result.IsSuccess)
                {
                    throw new BadRequestException($"Failed to deserialize: {result.Error}");
                }

                return result.Value!.FirstOrDefault(um => um.MovieId == movie.MvId) ?? new();
            }

            return userMovies[0];
        }
    }
}
