using System.Net;
using System.Net.Http.Headers;
using System.Text;
using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Dtos.Exceptions;
using Household.Shared.Helpers;
using Household.Shared.Models;
using MediatR;
using Newtonsoft.Json;
using Helpers = Household.Shared.Helpers;

namespace Household.Application.Features.Shows.Commands;

public class UpdateShow(TVShowDto show) : IRequest<string>
{
    public readonly TVShowDto UpdatedShow = show;

    public class UpdateShowCommandHandler(IHttpClientFactory httpClientFactory, IMapper mapper) : IRequestHandler<UpdateShow, string>
    {
        private readonly HttpClient? _client = httpClientFactory.CreateClient("ApiClient");
        private readonly IMapper _mapper = mapper;

        public async Task<string> Handle(UpdateShow request, CancellationToken cancellationToken)
        {
            if (_client == null)
                throw new BadRequestException("Cannot make client call to update data!");

            if (request.UpdatedShow.Id <= 0)
                throw new BadRequestException("Cannot be a new show!");

            TvShow? showDetail = _mapper.Map<TvShow>(request.UpdatedShow);
            if (showDetail != null)
            {
                showDetail.StreamingServiceId = request.UpdatedShow.StreamingId;
                showDetail.Time = Helpers.Common.GetLowerDateWithNewTime(request.UpdatedShow.Time, request.UpdatedShow.StartDate);
            }

            try
            {
                string content = JsonConvert.SerializeObject(showDetail);
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                ByteArrayContent byteContent = new (buffer);

                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await _client.PutAsync($"api/tvshows/{request.UpdatedShow.Id}", byteContent, cancellationToken);
                string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                    return response.StatusCode.ToString();
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
