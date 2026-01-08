namespace Household.Shared.Services.Interfaces;

public interface IPageHistoryState
{
    void AddPageToHistory(string pageName);
    bool CanGoBack();
    string GetGoBackPage();
}