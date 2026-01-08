using System.Net;
using Household.Shared.Dtos;
using Household.Shared.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Household.Shared.Services;

public class CachedTokenService : ITokenService
{
    private readonly IOptions<Authentication> _appSettings;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private const string CacheKey = "KeycloakToken";

    public CachedTokenService(IOptions<Authentication> appSettings, IMemoryCache cache)
    {
        _appSettings = appSettings;
        _cache = cache;
        _httpClient = new HttpClient();
    }

    public async Task<string> GetTokenAsync()
    {
        // Return token from cache if available
        if (_cache.TryGetValue(CacheKey, out string? token))
            return token ?? string.Empty;

        await _lock.WaitAsync();
        try
        {
            // Double-check caching to avoid race conditions
            if (_cache.TryGetValue(CacheKey, out token))
                return token ?? string.Empty;

            token = await FetchNewTokenAsync();

            // Store token with correct expiration time
            TimeSpan expiresIn = TimeSpan.FromSeconds(55 * 60); // default safety
            if (_cache.TryGetValue("TokenExpiresIn", out int seconds))
                expiresIn = TimeSpan.FromSeconds(seconds - 30); // refresh early

            _cache.Set(CacheKey, token, expiresIn);
            return token;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<string> FetchNewTokenAsync()
    {
        Keycloak scheme = _appSettings.Value.Schemes.KeycloakBackend;

        using HttpRequestMessage request = new (
            HttpMethod.Post,
            $"{scheme.Authority}/protocol/openid-connect/token");

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = scheme.ResponseType,
            ["client_id"] = scheme.ClientId,
            ["client_secret"] = scheme.ClientSecret
        });

        HttpResponseMessage response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            string details = await response.Content.ReadAsStringAsync();
            throw new UnauthorizedAccessException(
                $"Keycloak returned 401 Unauthorized during token retrieval. " +
                $"Check client_id/client_secret or Keycloak configuration. " +
                $"Response: {details}");
        }

        response.EnsureSuccessStatusCode();

        string? content = await response.Content.ReadAsStringAsync();

        Authorize auth = JsonConvert.DeserializeObject<Authorize>(content)
                   ?? throw new Exception("Unable to deserialize token response.");

        // Cache expiration seconds returned by Keycloak
        if (auth.ExpiresinSeconds > 0)
            _cache.Set("TokenExpiresIn", auth.ExpiresinSeconds);

        return auth.AccessToken;
    }
}
