using Household.Application.Features.Subscriptions.Commands;
using Household.Application.Features.Subscriptions.Queries;
using Household.Application.Interfaces;
using Household.Shared.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Household.Server.Controllers;


[Produces("application/json")]
[ApiController]
[Route("/api/[controller]")]
public class SubscriptionsController(IMediator mediator) : Controller, ISubscriptionsController
{
    [HttpPost]
    public async Task<string> CreateStreamingService(StreamingServiceDto request) => await mediator.Send(new CreateSubscription(request));

    [HttpGet]
    public async Task<List<StreamingServiceDto>> GetAllSubscriptions() => await mediator.Send(new GetAllSubscriptions());

    [HttpGet("{id}")]
    public async Task<StreamingServiceDto> GetSubscriptionById(int id) => await mediator.Send(new GetSubscriptionById(id));

    [HttpGet("Shows/{id}")]
    public async Task<List<TVShowDto>> GetAllShowsByServiceId(int id) => await mediator.Send(new GetAllShowsByServiceId(id));

    [HttpPut("UpdateService")]
    public async Task<string> PutTvShow(StreamingServiceDto updatedService) => await mediator.Send(new UpdateSubscription(updatedService));

    [HttpDelete("RemoveSubscription/{id:int}")]
    public async Task<string> DeleteSubscription(int id) => await mediator.Send(new DeleteSubscription(id));
}
