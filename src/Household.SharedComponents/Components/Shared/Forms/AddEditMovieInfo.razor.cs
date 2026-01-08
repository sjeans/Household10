using Blazorise;
using Household.Shared.Dtos;
using Household.Shared.Enums;
using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Shared.Forms;

public partial class AddEditMovieInfo : ComponentBase
{
    [Parameter]
    public int MovieId { get; set; }

    [Parameter]
    public MovieInfoDto MovieInfo { get; set; } = default!;

    [Parameter]
    public HttpClient? Client { get; set; } = default!;

    protected override void OnInitialized()
    {
        if (MovieId > 0)
            GetMovieInfoDetails();

    }

    private void GetMovieInfoDetails()
    {
        MovieInfoDto? movieInfo = MovieInfo;

        if (movieInfo != null)
        {
            MovieInfo = movieInfo;
            MovieId = movieInfo.Id;
        }
    }

    protected static Dictionary<string, object> HandyFunction()
    {
        Dictionary<string, object> dict = new()
        {
            { "autocomplete", true }
        };
        return dict;
    }

    protected void DiskTypeUpdate(int newDvdType)
    {
        MovieInfo.DvdType = (DvdTypes)newDvdType;
    }

    protected void EpisodeUpdate(int newEpisode)
    {
        MovieInfo.DiskNum = newEpisode;
    }

    protected static void ValidateDiskType(ValidatorEventArgs eventArgs)
    {
        bool selection = int.TryParse((string?)eventArgs.Value, out _);

        eventArgs.Status = selection ? ValidationStatus.Success : ValidationStatus.Error;
    }

    protected static void ValidateDiskNumber(ValidatorEventArgs eventArgs)
    {
        bool selection = int.TryParse((string?)eventArgs.Value, out _);

        eventArgs.Status = selection ? ValidationStatus.Success : ValidationStatus.Error;
    }
}
