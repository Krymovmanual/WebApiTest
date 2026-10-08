using Newtonsoft.Json;

namespace WebApiTest.API.Authorization;

public sealed class DevTokenResponse
{
    [JsonProperty("access_token")]
    public string? AccessToken { get; init; }

    [JsonProperty("token_type")]
    public string? TokenType { get; init; }

    [JsonProperty("expires_in")]
    public int? ExpiresIn { get; init; }

    [JsonProperty("refresh_token")]
    public string? RefreshToken { get; init; }

    [JsonProperty("scope")]
    public string? Scope { get; init; }

    [JsonProperty("error")]
    public string? Error { get; init; }

    [JsonProperty("error_description")]
    public string? ErrorDescription { get; init; }
}
