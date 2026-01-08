using HybridAuthDemo_Modal.Shared.Dtos;
using HybridAuthDemo_Modal.Shared.Services.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HybridAuthDemo_Modal.Shared.Services;

public class TokenService : ITokenService
{
    private readonly IOptions<AppSettings> _appSettings;
    private readonly HttpClient _httpClient;

    public TokenService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings;
        _httpClient = new HttpClient();
    }

    public async Task<string> GetTokenAsync()
    {
        Keycloak scheme = _appSettings.Value.Authentication.Schemes.KeycloakBackend;

        HttpRequestMessage request = new (
            HttpMethod.Post,
            $"{scheme.Authority}/protocol/openid-connect/token");

        List<KeyValuePair<string, string>> collection = new()
        {
            new("grant_type", scheme.ResponseType),
            new("client_id", scheme.ClientId),
            new("client_secret", scheme.ClientSecret),
        };

        request.Content = new FormUrlEncodedContent(collection);

        HttpResponseMessage response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();

        Authorize? authorize = JsonConvert.DeserializeObject<Authorize>(content);

        return authorize?.AccessToken ?? "";
    }
}
