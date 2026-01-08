namespace Household.Shared.Services.Interfaces;

public interface IApiRateLimiter
{
    SemaphoreSlim Semaphore { get; }
}
