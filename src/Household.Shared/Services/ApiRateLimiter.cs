using Household.Shared.Services.Interfaces;

namespace Household.Shared.Services;

public class ApiRateLimiter : IApiRateLimiter
{
    public SemaphoreSlim Semaphore { get; }

    public ApiRateLimiter(int maxConcurrency)
    {
        Semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    }
}
