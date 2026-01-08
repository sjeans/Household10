//using Household.Client.Helpers;
using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Models;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Household.SharedComponents.Components.Manage.Users.Forms.Selectors;

public partial class UserTypeSelector : ComponentBase
{
    [Parameter]
    public int UserTypeId { get; set; }

    [Parameter]
    public EventCallback<int> OnUserTypeUpdate { get; set; }

    [Parameter]
    public EventCallback<int> OnInValidSelection { get; set; }

    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private List<SelectOption> _options = [];
    private HttpClient? _client;

    protected override async Task OnInitializedAsync()
    {
        _client = ApiService.HttpClient;
        if (_options.Count == 0)
            await GetAllUserTypes();

    }

    protected async Task GetAllUserTypes()
    {
        if (_client == null)
            return;

        try
        {
            HttpRequestMessage request = new();
            HttpResponseMessage response = await _client.GetAsync("api/usertype");

            if (response.IsSuccessStatusCode)
            {
                await using Stream stream = await response.Content.ReadAsStreamAsync(new());
                Result<List<UserType>> result = await JsonDeserializer.TryDeserializeAsync<List<UserType>>(stream, new());

                if (!result.IsSuccess)
                {
                    //_logger.LogError("Failed to deserialize: {msg}", result.Error);
                    return;
                }

                List<UserType>? subscriptions = result.Value;

                if (subscriptions is List<UserType> results && results.Count > 0)
                {
                    results.ForEach(ss =>
                    {
                        SelectOption itemOption = new()
                        {
                            Text = ss.Description ?? string.Empty,
                            Value = ss.Id.ToString(),
                            Selected = ss.Id == UserTypeId,
                        };

                        _options.Add(itemOption);
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.GetInnerMessage());
        }
    }

    protected async Task<Task> HandleChange(ChangeEventArgs eventArgs)
    {
        string selectedOption = eventArgs.Value?.ToString() ?? string.Empty;

        if (!selectedOption.IsNullOrWhiteSpace())
        {
            _ = int.TryParse(selectedOption, out int selectedSeason);

            return OnUserTypeUpdate.InvokeAsync(selectedSeason);
        }

        if (int.TryParse(_options.FirstOrDefault(x => x.Selected)?.Value, out int oldState))
        {
            await JsRuntime.InvokeAsync<object>("ResetSelectValue", "ddlSubscriptions", oldState);

            return OnUserTypeUpdate.InvokeAsync(oldState);
        }

        return Task.CompletedTask;
    }
}
