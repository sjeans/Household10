using Household.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Household.SharedComponents.Components.Manage.Users;

public partial class UserEntry : ComponentBase
{
    [Parameter]
    public List<User> AllUsers { get; set; } = default!;

    private List<User> _users = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (AllUsers.Count > 0)
            _users = AllUsers;
    }
}
