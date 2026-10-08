using Newtonsoft.Json;

namespace WebApiTest.API.Configuration;

public sealed class DevCredentials
{
    [JsonProperty("userName")]
    public string UserName { get; init; } = string.Empty;

    [JsonProperty("password")]
    public string Password { get; init; } = string.Empty;
}
