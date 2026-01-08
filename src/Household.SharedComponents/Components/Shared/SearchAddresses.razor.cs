using Household.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Household.SharedComponents.Components.Shared.Loader;

namespace Household.SharedComponents.Components.Shared;

public partial class SearchAddresses
{
    [Parameter]
    public List<AddressInfoDto> AllAddresses { get; set; } = default!;

    [Inject] NavigationManager NavMan { get; set; } = default!;

    private List<char> _alphabet = [];
    private Loading _loadingIndicator = default!;

    protected override void OnInitialized()
    {
        _alphabet = GenerateAlphabet();
    }

    private async void DynamicUrl(string letter)
    {
        await _loadingIndicator.ShowAsync();
        if (letter != "All")
            NavMan.NavigateTo($"/addressbook/contact/{letter}", true);
        else
            NavMan.NavigateTo($"/addressbook", true);

        await _loadingIndicator.HideAsync();
    }

    static List<char> GenerateAlphabet()
    {
        List<char> alphabet = [];

        for (char letter = 'A'; letter <= 'Z'; letter++)
            alphabet.Add(letter);

        return alphabet;
    }
}
