using Household.Shared.Dtos;

namespace Household.Application.Interfaces;

public interface ISubscriptionsController
{
    Task<string> CreateStreamingService(StreamingServiceDto request);
    Task<string> DeleteSubscription(int id);
    Task<List<StreamingServiceDto>> GetAllSubscriptions();
    Task<List<TVShowDto>> GetAllShowsByServiceId(int id);
    Task<StreamingServiceDto> GetSubscriptionById(int id);
    Task<string> PutTvShow(StreamingServiceDto updatedService);
}
