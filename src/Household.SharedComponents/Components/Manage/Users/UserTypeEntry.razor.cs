using Household.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Manage.Users;

public partial class UserTypeEntry : ComponentBase
{
    [Parameter]
    public List<UserType> UserTypes { get; set; } = new();

    private List<UserType> _allUserTypes = default!;
    
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (UserTypes.Count > 0)
        {
            _allUserTypes = UserTypes;
        }
    }
}
