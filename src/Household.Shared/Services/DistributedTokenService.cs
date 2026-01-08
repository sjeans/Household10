using Household.Shared.Dtos;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Household.Shared.Services;


public class DistributedTokenService : ITokenService
{
    private readonly IOptions<Authentication> _appSettings;
    private readonly IDistributedCache _cache;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private const string ACCESS_TOKEN_KEY = CacheKeys.AccessToken;
    private const string EXPIRATION_KEY = CacheKeys.ExpiresAt;

    public DistributedTokenService(IOptions<Authentication> appSettings, IDistributedCache cache)
    {
        _appSettings = appSettings;
        _cache = cache;
        _httpClient = new HttpClient();
    }

    public async Task<string> GetTokenAsync()
    {
        //string? expiresAtString = await _cache.GetRecordAsync<string>(EXPIRATION_KEY);
        string? token = await _cache.GetRecordAsync<string>(ACCESS_TOKEN_KEY);

        if (!string.IsNullOrEmpty(token) /*&& DateTime.UtcNow < DateTime.Parse(expiresAtString ?? "2000-01-01")*/)
        {
            return token;
        }

        await _lock.WaitAsync();
        try
        {
            // Re-check after entering lock
            token = await _cache.GetRecordAsync<string>(ACCESS_TOKEN_KEY);
            //expiresAtString = await _cache.GetRecordAsync<string>(EXPIRATION_KEY);

            if (!string.IsNullOrEmpty(token) /*&& DateTime.UtcNow < DateTime.Parse(expiresAtString ?? "2000-01-01")*/)
            {
                return token;
            }

            return await FetchAndCacheNewTokenAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<string> FetchAndCacheNewTokenAsync()
    {
        Keycloak scheme = _appSettings.Value.Schemes.KeycloakBackend;

        using HttpRequestMessage request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{scheme.Authority}/protocol/openid-connect/token");

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = scheme.ResponseType,
            ["client_id"] = scheme.ClientId,
            ["client_secret"] = scheme.ClientSecret
        });

        using HttpResponseMessage response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        Authorize auth = JsonConvert.DeserializeObject<Authorize>(json)
                   ?? throw new Exception("Failed to parse Keycloak token response");

        DateTime expiresAt = DateTime.UtcNow.AddSeconds(auth.ExpiresinSeconds - 30);

        // Cache the token
        await _cache.SetRecordAsync(ACCESS_TOKEN_KEY, auth.AccessToken);
        //await _cache.SetRecordAsync(EXPIRATION_KEY, expiresAt.ToString("O"));

        return auth.AccessToken;
    }
}
