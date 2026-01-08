using Household.Shared.Dtos;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Serilog;
using Household.SharedComponents.Components.Shared.Loader;

namespace Household.SharedComponents.Components.Pages.Managing.ManageUserMovies;

public partial class Add
{
    [Inject] private IApiService ApiService { get; set; } = default!;
    [Inject] private ILogger Logger { get; set; } = default!;
    [Inject] private IAppJsonDeserializer JsonDeserializer { get; set; } = default!;

    private Loading _loadingIndicator = default!;

    private UserIpDto _userIp = default!;
    private HttpClient? _client;
    private ILogger _logger = default!;
}
