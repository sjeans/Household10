using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Household.SharedComponents.Components.Shared;

public partial class Search : ComponentBase
{
    [Parameter]
    public string Term { get; set; } = string.Empty;

    [Inject] protected NavigationManager NavManager { get; set; } = default!;

    private string _searchTerm = string.Empty;

    private void UpdateSearchTerm(ChangeEventArgs e)
    {
        _searchTerm = e.Value?.ToString() ?? string.Empty;
        // Additional logic can go here if needed
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            SearchIssues();
        }
    }

    protected void SearchIssues()
    {
        Term = _searchTerm;
        if (!_searchTerm.IsNullOrWhiteSpace())
            NavManager.NavigateTo($"/FoundShows/Index/{_searchTerm.ToLower()}", true);

    }
}
