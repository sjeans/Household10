using Blazorise;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Serilog;

namespace Household.SharedComponents.Components.Shared.Forms;

public partial class AddEditUser : ComponentBase, IDisposable
{
    [CascadingParameter]
    public EditContext EditContextRef { get; set; } = default!;

    [Parameter]
    public required int UserId { get; set; }

    [Parameter]
    public required User UserRef { get; set; }

    [Parameter]
    public bool CanSave { get; set; }

    [Parameter]
    public string CanShow { get; set; } = default!;

    [Parameter]
    public string Message { get; set; } = default!;

    [Parameter]
    public string ClientId { get; set; } = default!;

    [Parameter]
    public HttpClient Client { get; set; } = default!;

    [Parameter]
    public Messages.Notification ComponentNotification { get; set; } = default!;

    [Parameter]
    public ILogger? Logger { get; set; }

    [Inject] private ILogger DefaultLogger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    // Pick whichever is available
    private ILogger ActiveLogger => Logger ?? DefaultLogger;

    private User? _originalUser = default!;
    private User _editUser = default!;
    private HttpClient? _client = default;

    private string _userName = string.Empty;
    private readonly string _message = string.Empty;
    private string _canShow = string.Empty;
    private string _clientId = string.Empty;
    private bool _canSave = false;

    private bool disposedValue;

    public AddEditUser(ILogger logger)
    {
        DefaultLogger = logger;
    }

    protected override void OnParametersSet() // OnInitialized()
    {
        base.OnParametersSet();
        ActiveLogger.Information("Initializing add/edit user.");

        GetUserDetails();
        _canSave = CanSave;
        _canShow = CanShow;
        _clientId = ClientId;
        _client = Client;
    }

    //protected override async Task OnInitializedAsync()
    //{
    //    await base.OnInitializedAsync();
    //    _logger = LoggerFactory.CreateLogger<AddEditUser>();
    //    _logger.LogInformation("Initializing add/edit user.");

    //    await GetUserDetails();

    //    if (EditContextRef is null && UserId > 0)
    //    {
    //        EditContextRef = new EditContext(_editUser!);
    //    }

    //    if (!_editContextInitialized && EditContextRef is not null)
    //    {
    //        EditContextRef.OnFieldChanged += EditContext_OnFieldChanged;
    //        _editContextInitialized = true;
    //    }

    //    if (UserId > 0)
    //    {
    //        ValidationContext validationContext = new(_editUser!);
    //    }
    //}

    //private async Task GetUserDetails()
    //{
    //    if (Client == null)
    //    {
    //        _logger.LogError("Cannot make client call to retrieve data!");
    //        return;
    //    }

    //    try
    //    {
    //        _logger.LogInformation("Retrieving user to edit.");
    //        HttpResponseMessage response = await Client.GetAsync($"api/user/{UserId}");

    //        User user = new();
    //        if (response.IsSuccessStatusCode)
    //        {
    //            await using Stream stream = response.Content.ReadAsStream(new());
    //            ImprovedResult<User> result = await JsonDeserializer.TryDeserializeAsync<User>(stream, new());

    //            if (!result.IsSuccess)
    //            {
    //                _logger.LogError("Failed to deserialize: {msg}", result.Error);
    //                return;
    //            }

    //            _logger.LogInformation("Retrieved user to edit.");
    //            user = result.Value!;
    //        }

    //        if (user != null)
    //        {
    //            _logger.LogInformation("Setting up original state.");
    //            _originalUser = new()
    //            {
    //                Id = user.Id,
    //                Active = user.Active,
    //                Email = user.Email,
    //                FirstName = user.FirstName,
    //                LastName = user.LastName,
    //                Password = user.Password,
    //                UserName = user.UserName,
    //                UserTypeId = user.UserTypeId,
    //                UserType = user.UserType,
    //            };

    //            _editUser = user;
    //            _userName = user.FirstName + " " + user.LastName;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Encountered error retrieving user to edit. Error: {errMsg}", ex.Message);
    //    }
    //}

    private void GetUserDetails()
    {
        try
        {
            _editUser = UserRef;

            if (_editUser != null)
            {
                ActiveLogger.Information("Setting up original state.");
                _originalUser = new()
                {
                    Id = _editUser.Id,
                    Active = _editUser.Active,
                    Email = _editUser.Email,
                    FirstName = _editUser.FirstName,
                    LastName = _editUser.LastName,
                    Password = _editUser.Password,
                    UserName = _editUser.UserName,
                    UserTypeId = _editUser.UserTypeId,
                    UserType = _editUser.UserType,
                };

                _userName = _editUser.FirstName + " " + _editUser.LastName;
            }
        }
        catch (Exception ex)
        {
            ActiveLogger.Error(ex, "Encountered error retrieving user to edit. Error: {errMsg}", ex.Message);
        }
    }

    protected void UpdateProperty(string propertyName, int newValue)
    {
        if (propertyName == "UserType")
            _editUser!.UserTypeId = newValue;

        EditContextRef.NotifyFieldChanged(EditContextRef.Field(propertyName));
    }

    protected void UserTypeUpdate(int newUserType) => UpdateProperty("UserType", newUserType);

    protected static void ValidateProperty(ValidatorEventArgs eventArgs)
    {
        bool selection = int.TryParse((string?)eventArgs.Value, out _);

        eventArgs.Status = selection ? ValidationStatus.Success : ValidationStatus.Error;
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
    ~AddEditUser()
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
