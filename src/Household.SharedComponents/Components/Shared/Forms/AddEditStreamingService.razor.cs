using System.ComponentModel.DataAnnotations;
using Blazorise;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Serilog;
using Household.SharedComponents.Components.Shared.Modals;

namespace Household.SharedComponents.Components.Shared.Forms;

public partial class AddEditStreamingService : ComponentBase
{
    [Parameter]
    public int StreamingServiceId { get; set; }

    [Parameter]
    public StreamingServiceDto ServiceDto { get; set; }

    [Parameter]
    public bool CanSave { get; set; }

    [Parameter]
    public string CanShow {  get; set; } = default!;

    [Parameter]
    public HttpClient? Client { get; set; }

    [Parameter]
    public ILogger? Log { get; set; }

    [Parameter]
    public Notification ComponentNotification { get; set; } = default!;

    [Parameter]
    public string Message { get; set; } = default!;

    public string StreamingName { get; private set; }
    public EditContext EditContext { get; private set; } = default!;

    private bool _editContextInitialized;

    public AddEditStreamingService()
    {
        StreamingServiceId = 0;
        StreamingName = string.Empty;
        ServiceDto = new();
    }

    public AddEditStreamingService(int id)
    {
        StreamingServiceId = id;
        StreamingName = string.Empty;
        ServiceDto = new();
    }

    protected override void OnParametersSet() // OnInitialized()
    {
        if (StreamingServiceId > 0)
            GetStreamingServiceDetails();

        if (EditContext is null)
        {
            EditContext = new EditContext(ServiceDto);
        }

        if (!_editContextInitialized && EditContext is not null)
        {
            EditContext.OnFieldChanged += EditContext_OnFieldChanged;
            _editContextInitialized = true;
        }
        //EditContext.OnFieldChanged += EditContext_OnFieldChanged;
        ValidationContext validationContext = new (ServiceDto);
    }

    private void GetStreamingServiceDetails()
    {
        StreamingServiceDto? streamingService = ServiceDto;

        if (streamingService != null)
        {
            ServiceDto = streamingService;
            StreamingName = streamingService.Name;
        }
    }

    protected async Task DeleteSubscription()
    {
        if (Client == null)
            return;

        try
        {
            Log?.Information("Deleting the subscription {name}.", ServiceDto.Name);

            HttpResponseMessage response = await Client.DeleteAsync($"api/subscriptions/removesubscription/{ServiceDto.Id}");
            string responseString = await response.Content.ReadAsStringAsync();

            Log?.Information("Deleted subscription: {status}.", response.IsSuccessStatusCode);
            Message = response.StatusCode.ToString();

            if (response.IsSuccessStatusCode)
            {
                Log?.Information("You have successfully deleted the subscription {name}!", ServiceDto.Name);
                ComponentNotification.Show(1, true, "You have successfully deleted the service!");
            }
            else
            {
                Log?.Error("{msg}", responseString);
                ComponentNotification.Show(2, false, $"{Message}");
            }

        }
        catch (Exception ex)
        {
            Log?.Error(ex, "Encountered an error deleting show. Error: {errMsg}", ex.GetInnerMessage());
            ComponentNotification.Show(3, false, $"{Message}");
        }
    }

    // Note: The OnFieldChanged event is raised for each field in the model
    private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        Console.WriteLine(e.FieldIdentifier.FieldName);
    }

    protected static void ValidateDate(ValidatorEventArgs eventArgs)
    {
        bool selection = DateTime.TryParse((string?)eventArgs.Value, out DateTime selected);

        if (DateTime.Compare(selected, DateTime.Now) < 0)
        {
            eventArgs.Status = ValidationStatus.Error;
            return;
        }

        eventArgs.Status = selection ? ValidationStatus.Success : ValidationStatus.Error;
    }
}
