using System.ComponentModel.DataAnnotations;
using Household.SharedComponents.Components.Shared;
using Household.SharedComponents.Components.Shared.Forms;
using Household.SharedComponents.Components.Shared.Loader;
using Household.SharedComponents.Components.Shared.Modals;
using Household.Shared.Dtos;
using Household.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Serilog;

namespace Household.SharedComponents.Components.Manage.Users.Forms;

public partial class AddEditUserType : ComponentBase, IDisposable
{
    [CascadingParameter]
    private EditContext EditContextRef { get; set; } = new(new User());

    [Parameter]
    public UserType UserType { get; set; } = new();

    [Parameter]
    public Notification Notification { get; set; } = new();

    [Parameter]
    public string CanShow { get; set; } = default!;

    [Parameter]
    public string ClientId { get; set; } = default!;

    [Parameter]
    public bool CanSave { get; set; }

    // Optional: parent can pass its own logger
    [Parameter] public ILogger? Logger { get; set; }

    [Inject] private ILogger DefaultLogger { get; set; } = default!;

    // Pick whichever is available
    private ILogger ActiveLogger => Logger ?? DefaultLogger;
    private UserType _userType = default!;
    private readonly HttpClient? _client;

    private readonly string _roleName = string.Empty;
    private string _canShow = string.Empty;
    private string _clientId = string.Empty;
    private bool _canSave = false;
    private bool disposedValue;

    public AddEditUserType(ILogger logger)
    {
        DefaultLogger = logger;
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        ActiveLogger.Information("Parameters setup for add/editing user types");
        _canSave = CanSave;
        _canShow = CanShow;
        _clientId = ClientId;

        if (UserType is not null)
        {
            _userType = UserType;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                if (EditContextRef is not null)
                {
                    EditContextRef.OnFieldChanged -= null;
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~AddEditUserType()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
