using Household.Shared.Dtos;
using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] NavigationManager NavManager { get; set; } = default!;

    [CascadingParameter]
    protected DateTime Pickday { get; set; } = DateTime.Now.Date;

    protected DayOfWeek DayOfWeek { get; set; }
    protected StreamingServiceDto TVShow { get; set; } = new StreamingServiceDto();

    protected override async Task OnInitializedAsync()
    {
        DayOfWeek = Pickday.DayOfWeek;
        TVShow.PaySchedule = DateTime.Now.Date;
    }

    public void GoToDay(DateTime selectedDay)
    {
        //NavManager.NavigateTo($"/DailyShows/ShowsForDay/{selectedDay.Date.Ticks}");
        NavManager.NavigateTo($"/dailyshows/showsforday/{selectedDay.Date:d}");
    }
}
