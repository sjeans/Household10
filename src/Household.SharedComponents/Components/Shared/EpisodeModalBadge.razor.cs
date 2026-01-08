using Blazorise;
using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Shared;

public partial class EpisodeModalBadge : ComponentBase
{
    [Parameter]
    public Modal ModalRef { get; set; } = default!;

    [Parameter]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public EventCallback OnClick { get; set; }

    private string _badgeBackground = string.Empty;
    private string _badgeTextColor = string.Empty;

    protected override void OnParametersSet()
    {
        // Initialize or compute derived values
        _badgeBackground = EpisodeBadge.BackgroundLight;
        _badgeTextColor = EpisodeBadge.TextColorLight;

        if (Label.IsNullOrWhiteSpace())
        {
            return;
        }

        switch (Label)
        {
            case EpisodeText.LastEpisodeLabel:
                _badgeBackground = EpisodeBadge.BackgroundDanger;
                break;
            case EpisodeText.SeasonFinaleLabel:
                _badgeBackground = EpisodeBadge.BackgroundWarning;
                _badgeTextColor = EpisodeBadge.TextColorDark;
                break;
            case EpisodeText.ComingSoonLabel:
                _badgeBackground = EpisodeBadge.BackgroundSuccess;
                break;
            default:
                _badgeBackground = EpisodeBadge.BackgroundInfo;
                _badgeTextColor = EpisodeBadge.TextColorDark;
                break;
        }
    }

    private async Task HandleClick()
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync(null);

    }
}
