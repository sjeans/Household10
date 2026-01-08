using Newtonsoft.Json;

namespace Household.Shared.Dtos;

public record Authorize
{
    [JsonProperty("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonProperty("expires_in")]
    public int ExpiresinSeconds { get; init; }

    [JsonProperty("refresh_expires_in")]
    public string RefreshTokenExpiresinSeconds { get; init; } = string.Empty;

    [JsonProperty("token_type")]
    public string TokenType { get; init; } = string.Empty;

    //[JsonProperty("proxy_id")]
    //public string ProxyId { get; init; } = string.Empty;

    [JsonProperty("not-before-policy")]
    public string NotBeforePolicy { get; init; } = string.Empty;


    [JsonProperty("scope")]
    public string Scope { get; init; } = string.Empty;
}
