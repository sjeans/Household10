namespace Household.Shared.Services;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

public class FinalDistributedTokenService /*: IDistributedTokenService*/
{
    private readonly IConfiguration _config;
    private readonly IDistributedCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<DistributedTokenService> _logger;

    public FinalDistributedTokenService(
        IConfiguration config,
        IDistributedCache cache,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<DistributedTokenService> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // -------------------------------------------------------------------------
    // Cache key generation: supports multi-user + multi-client (+ optional tenant)
    // -------------------------------------------------------------------------
    private static string GetAccessTokenKey(string clientId, string userId, string? tenantId = null)
        => $"Keycloak:{tenantId ?? "Default"}:{clientId}:{userId}:AccessToken";

    private static string GetExpiresAtKey(string clientId, string userId, string? tenantId = null)
        => $"Keycloak:{tenantId ?? "Default"}:{clientId}:{userId}:ExpiresAt";

    // -------------------------------------------------------------------------
    // Public entrypoint
    // -------------------------------------------------------------------------
    public async Task<string> GetTokenAsync(string clientId, string? tenantId = null)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            throw new InvalidOperationException("No authenticated user to retrieve a token for.");

        return await GetOrRefreshTokenAsync(clientId, userId, tenantId);
    }

    // -------------------------------------------------------------------------
    // Internal token handling
    // -------------------------------------------------------------------------
    private async Task<string> GetOrRefreshTokenAsync(string clientId, string userId, string? tenantId)
    {
        var tokenKey = GetAccessTokenKey(clientId, userId, tenantId);
        var expirationKey = GetExpiresAtKey(clientId, userId, tenantId);

        string? existingToken = await _cache.GetStringAsync(tokenKey);
        string? expiresAtString = await _cache.GetStringAsync(expirationKey);

        // If token exists and is not expired
        if (existingToken != null && DateTime.TryParse(expiresAtString, out var expiresAt))
        {
            if (expiresAt > DateTime.UtcNow.AddMinutes(1))
            {
                return existingToken;
            }
        }

        // Otherwise refresh
        return await RefreshTokenAsync(clientId, userId, tenantId, tokenKey, expirationKey);
    }

    private async Task<string> RefreshTokenAsync(
        string clientId,
        string userId,
        string? tenantId,
        string tokenKey,
        string expirationKey)
    {
        var keycloakSettings = _config.GetSection("Keycloak");
        string tokenUrl = keycloakSettings["TokenEndpoint"]
            ?? throw new InvalidOperationException("Missing Keycloak:TokenEndpoint in config.");

        string clientSecret = keycloakSettings[$"Clients:{clientId}:Secret"]
            ?? throw new InvalidOperationException($"Missing Keycloak client secret for '{clientId}'.");

        string scope = keycloakSettings[$"Clients:{clientId}:Scope"] ?? "openid";

        var client = _httpClientFactory.CreateClient();
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["scope"] = scope
        };

        var response = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(form));

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError("Keycloak returned 401 Unauthorized for client {ClientId}.", clientId);
            throw new UnauthorizedAccessException("Could not authenticate with Keycloak (401).");
        }

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(json)
            ?? throw new Exception("Invalid Keycloak token response.");

        // Save to distributed cache
        await _cache.SetStringAsync(tokenKey, tokenResponse.AccessToken);
        var expiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

        await _cache.SetStringAsync(expirationKey, expiresAt.ToString("O"));

        return tokenResponse.AccessToken;
    }

    // -------------------------------------------------------------------------
    // User identification
    // -------------------------------------------------------------------------
    private string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?
            .User?
            .FindFirstValue(ClaimTypes.NameIdentifier) // often "sub" in Keycloak
            ?? _httpContextAccessor.HttpContext?
                .User?
                .FindFirstValue("sub");
    }
}

public class KeycloakTokenResponse
{
    public string AccessToken { get; set; } = default!;
    public int ExpiresIn { get; set; }
}

