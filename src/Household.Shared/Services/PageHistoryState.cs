using Household.Shared.Services.Interfaces;

namespace Household.Shared.Services;

public class PageHistoryState : IPageHistoryState
{
    private readonly List<string>? previousPages;

    public PageHistoryState()
    {
        previousPages = [];
    }
    public void AddPageToHistory(string pageName)
    {
        previousPages?.Add(pageName);
    }

    public string GetGoBackPage()
    {
        if (previousPages?.Count > 0)
        {
            // You add a page on initialization, so you need to return the 2nd from the last
            string goToPage = previousPages.ElementAt(previousPages.Count - 1);
            //previousPages.RemoveAt(previousPages.Count - 1);
            return goToPage;
        }

        // Can't go back because you didn't navigate enough
        return previousPages?.FirstOrDefault() ?? string.Empty;
    }

    public bool CanGoBack()
    {
        return previousPages?.Count > 1;
    }
}
