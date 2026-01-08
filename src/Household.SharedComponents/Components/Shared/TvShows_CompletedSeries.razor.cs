using Household.Shared.Dtos;
using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Shared;

public partial class TvShows_CompletedSeries
{
    [Parameter]
    public List<TVShowDto> TvShowCategory { get; set; } = default!;

    [Parameter]
    public string LinkDisabled { get; set; } = string.Empty;

    [Parameter]
    public List<TVShowInformationDto>? ShowInformation { get; set; } = default!;
}
