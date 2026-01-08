using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Services.Interfaces;
using MediatR;

namespace Household.Application.Features.UserIpService.Query;

public class GetUserIp() : IRequest<UserIpDto>
{
    //public string Name { get; }
    //public string CanShow { get; }
    //public string IpAddress { get; }
    //public string PermissionSetBy { get; }
    //public string DisableButton { get; }
    //public string Visible { get; }
    //public string UrlReferrer { get; }
    //public string LogMessage { get; }
    //public bool CanSave { get; }

    //private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;


    public class GetUserIpQueryHandler : IRequestHandler<GetUserIp, UserIpDto>
    {
        private readonly IUserIpService _userIpService;
        private readonly IMapper _mapper;

        public GetUserIpQueryHandler(IUserIpService userIpService, IMapper mapper)
        {
            _userIpService = userIpService;
            _mapper = mapper;
        }

        public Task<UserIpDto> Handle(GetUserIp request, CancellationToken cancellationToken)
        {
            // Call the service to retrieve IP details.
            _userIpService.GetUserIP();

            // Map the service's state to a DTO.
            UserIpDto result = new UserIpDto
            {
                CanShow = _userIpService.CanShow,
                IpAddress = _userIpService.IpAddress,
                PermissionSetBy = _userIpService.PermissionSetBy,
                DisableButton = _userIpService.DisableButton,
                Visible = _userIpService.Visible,
                UrlReferrer = _userIpService.UrlReferrer,
                LogMessage = _userIpService.LogMessage,
                CanSave = _userIpService.CanSave
            };

            return Task.FromResult(result);
        }
    }
}
