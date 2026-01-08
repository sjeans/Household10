using Household.Shared.Services.Interfaces;

namespace Household.Shared.Helpers;

public class KeycloakAuthHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;

    public KeycloakAuthHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        //string token = await _tokenService.GetTokenAsync("tvshow-api");
        string token = await _tokenService.GetTokenAsync();

        request.Headers.TryAddWithoutValidation("Accept", "multipart/form-data");
        request.Headers.TryAddWithoutValidation("User-Agent", "HttpClientFactory");

        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
