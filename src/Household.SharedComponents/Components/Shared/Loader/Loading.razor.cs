using Blazorise.LoadingIndicator;
using Blazorise.SpinKit;
using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Shared.Loader;

public partial class Loading : ComponentBase
{

    // Optional: parameters for customization
    [Parameter]
    public string Class { get; set; } = "mt-3";

    [Parameter]
    public string IndicatorContainerClass { get; set; } = "mb-5";

    [Parameter]
    public SpinKitType Type { get; set; } = SpinKitType.CircleFade;

    [Parameter]
    public string Size { get; set; } = "1.7rem";

    [Parameter]
    public string Color { get; set; } = "#70082B";  //background-color: rgba(51, 51, 51, 0.7); #068705

    [Parameter]
    public bool IsLoading { get; set; } = true;

    [Parameter]
    public LoadingIndicator LoadingIndicator { get; set; } = default!;

    private LoadingIndicator _loadingIndicator = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        // You can set a default loading text or any other initialization logic here
        _loadingIndicator = LoadingIndicator ?? new LoadingIndicator();
    }

    public async Task ShowAsync() => await _loadingIndicator.Show();

    public async Task HideAsync() => await _loadingIndicator.Hide();

    public bool IsVisible => _loadingIndicator?.Visible ?? false;
}
