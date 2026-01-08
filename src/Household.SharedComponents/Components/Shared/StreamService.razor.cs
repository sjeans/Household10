using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Shared;

public partial class StreamService
{
    [Parameter]
    public StreamingServiceDto StreamingService { get; set; } = default!;

    [Parameter]
    public string Disable { get; set; } = string.Empty;

    protected static string GetSubscription(string? subscription)
    {
        return subscription.IsNullOrWhiteSpace() ? string.Empty : subscription;
    }
}
